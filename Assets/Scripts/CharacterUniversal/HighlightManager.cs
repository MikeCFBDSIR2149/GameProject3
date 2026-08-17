using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;
using Unity.Profiling;

namespace CharacterUniversal
{
    [DefaultExecutionOrder(500)]
    public class HighlightManager : MonoSingleton<HighlightManager>
    {
        private const int PrewarmCount = 100;
        private const int PrewarmPerFrame = 10;
        private const int MaxRetainedUI = 128;
        private const int MaxOcclusionChecksPerFrame = 20;
        private static readonly ProfilerMarker UpdateMarker =
            new ProfilerMarker("HighlightManager.UpdateHighlights");

        private sealed class HighlightEntry
        {
            public IHighlightInViewport Owner;
            public MonoBehaviour OwnerBehaviour;
            public Transform Transform;
            public IHighlightOcclusionSource OcclusionSource;
            public IHighlightUI UI;
            public bool IsHighlighted;
            public bool CachedOccluded;
            public float NextOcclusionCheckTime;
            public ulong RegistrationOrder;
        }

        private sealed class HighlightUIPool
        {
            public readonly Stack<IHighlightUI> Available = new Stack<IHighlightUI>(PrewarmCount);
            public int TotalCount;
            public bool PrewarmDisabled;
        }

        private readonly List<HighlightEntry> _entries = new List<HighlightEntry>(256);
        private readonly Dictionary<IHighlightInViewport, int> _entryIndices =
            new Dictionary<IHighlightInViewport, int>(256);
        private readonly Dictionary<string, HighlightUIPool> _uiPools =
            new Dictionary<string, HighlightUIPool>();

        private Canvas _highlightCanvas;
        private Coroutine _prewarmCoroutine;
        private IHighlightInViewport _bestTarget;
        private MonoBehaviour _bestTargetBehaviour;
        private float _bestAimScore;
        private int _bestPriority;
        private ulong _bestRegistrationOrder;
        private ulong _nextRegistrationOrder;

        public int RegisteredCount => _entries.Count;
        public int ActiveHighlightCount { get; private set; }
        public int LastFrameOcclusionChecks { get; private set; }

        private void OnEnable()
        {
            GameplayManager gameplayManager = GameplayManager.Instance;
            if (gameplayManager != null)
                gameplayManager.OnStatusChanged += OnGameplayStatusChanged;

            LevelManager levelManager = LevelManager.Instance;
            if (levelManager != null)
                levelManager.BeforeSceneLoad += HandleBeforeSceneLoad;
        }

        private void OnDisable()
        {
            GameplayManager gameplayManager = GameplayManager.Instance;
            if (gameplayManager != null)
                gameplayManager.OnStatusChanged -= OnGameplayStatusChanged;

            LevelManager levelManager = LevelManager.Instance;
            if (levelManager != null)
                levelManager.BeforeSceneLoad -= HandleBeforeSceneLoad;

            HideAllHighlights();
        }

        public void Register(IHighlightInViewport highlightObject)
        {
            if (highlightObject == null || _entryIndices.ContainsKey(highlightObject))
                return;

            Transform highlightTransform = highlightObject.HighlightTransform;
            if (highlightTransform == null || highlightObject is not MonoBehaviour ownerBehaviour)
                return;

            float interval = 0f;
            IHighlightOcclusionSource occlusionSource = highlightObject as IHighlightOcclusionSource;
            if (occlusionSource != null && occlusionSource.UseHighlightOcclusion)
                interval = Mathf.Max(0.01f, occlusionSource.HighlightOcclusionInterval);

            ulong order = _nextRegistrationOrder++;
            HighlightEntry entry = new HighlightEntry
            {
                Owner = highlightObject,
                OwnerBehaviour = ownerBehaviour,
                Transform = highlightTransform,
                OcclusionSource = occlusionSource,
                RegistrationOrder = order,
                NextOcclusionCheckTime = Time.unscaledTime + interval * ((order % PrewarmCount) / (float)PrewarmCount)
            };

            _entryIndices.Add(highlightObject, _entries.Count);
            _entries.Add(entry);

            GetOrCreatePool(highlightObject.HighlightUIPrefabName);
            StartPrewarmIfNeeded();
        }

        public void Unregister(IHighlightInViewport highlightObject)
        {
            if (highlightObject == null || !_entryIndices.TryGetValue(highlightObject, out int index))
                return;

            RemoveEntryAt(index, true);
        }

        public bool TryGetBestTarget(out IHighlightInViewport target)
        {
            if (_bestTarget != null && _bestTargetBehaviour != null &&
                _bestTargetBehaviour.isActiveAndEnabled && _entryIndices.ContainsKey(_bestTarget))
            {
                target = _bestTarget;
                return true;
            }

            target = null;
            return false;
        }

        private void LateUpdate()
        {
            using (UpdateMarker.Auto())
                UpdateHighlights();
        }

        private void UpdateHighlights()
        {
            ResetBestTarget();
            LastFrameOcclusionChecks = 0;

            GameplayManager gameplayManager = GameplayManager.Instance;
            if (gameplayManager == null || gameplayManager.Status != EGameplayStatus.BulletTime)
            {
                if (ActiveHighlightCount > 0)
                    HideAllHighlights();
                return;
            }

            Player.Player player = gameplayManager.Player;
            if (!player)
            {
                if (ActiveHighlightCount > 0)
                    HideAllHighlights();
                return;
            }

            Camera camera = player.GetPlayerCamera();
            if (!camera)
            {
                if (ActiveHighlightCount > 0)
                    HideAllHighlights();
                return;
            }

            Vector3 playerPosition = player.GetWorldPosition();
            Vector3 cameraPosition = camera.transform.position;
            Vector2 aimPoint = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            float now = Time.unscaledTime;
            int remainingOcclusionChecks = MaxOcclusionChecksPerFrame;

            for (int i = 0; i < _entries.Count;)
            {
                HighlightEntry entry = _entries[i];
                if (entry.OwnerBehaviour == null || entry.Transform == null)
                {
                    RemoveEntryAt(i, false);
                    continue;
                }

                if (!entry.OwnerBehaviour.isActiveAndEnabled || !entry.Owner.IsHighlightEligible)
                {
                    HideEntry(entry, true);
                    i++;
                    continue;
                }

                float minDistance = Mathf.Max(0f, entry.Owner.HighlightMinDistance);
                float maxDistance = Mathf.Max(minDistance, entry.Owner.HighlightMaxDistance);
                Vector3 playerDelta = entry.Transform.position - playerPosition;
                float squaredDistance = playerDelta.sqrMagnitude;

                if (squaredDistance < minDistance * minDistance || squaredDistance > maxDistance * maxDistance)
                {
                    HideEntry(entry, true);
                    i++;
                    continue;
                }

                float distance = Mathf.Sqrt(squaredDistance);
                float distanceRatio = Mathf.Approximately(minDistance, maxDistance)
                    ? 0f
                    : Mathf.Clamp01(Mathf.InverseLerp(minDistance, maxDistance, distance));

                Vector3 screenPosition = camera.WorldToScreenPoint(entry.Transform.position);
                if (screenPosition.z <= 0f || screenPosition.x < 0f || screenPosition.x > Screen.width ||
                    screenPosition.y < 0f || screenPosition.y > Screen.height)
                {
                    HideEntry(entry, true);
                    i++;
                    continue;
                }

                IHighlightOcclusionSource occlusionSource = entry.OcclusionSource;
                if (occlusionSource != null && occlusionSource.UseHighlightOcclusion)
                {
                    if (now >= entry.NextOcclusionCheckTime && remainingOcclusionChecks > 0)
                    {
                        entry.CachedOccluded = IsOccluded(entry, occlusionSource, cameraPosition);
                        entry.NextOcclusionCheckTime = now + Mathf.Max(0.01f, occlusionSource.HighlightOcclusionInterval);
                        remainingOcclusionChecks--;
                        LastFrameOcclusionChecks++;
                    }

                    if (entry.CachedOccluded)
                    {
                        HideEntry(entry, true);
                        i++;
                        continue;
                    }
                }
                else
                {
                    entry.CachedOccluded = false;
                }

                if (!ShowOrUpdateEntry(entry, screenPosition, distanceRatio))
                {
                    i++;
                    continue;
                }

                EvaluateAimTarget(entry, aimPoint);
                i++;
            }
        }

        private static bool IsOccluded(HighlightEntry entry, IHighlightOcclusionSource source, Vector3 origin)
        {
            Vector3 direction = entry.Transform.position - origin;
            float maxDistance = direction.magnitude;
            if (maxDistance <= Mathf.Epsilon)
                return false;

            if (!Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance,
                    source.HighlightOcclusionMask, QueryTriggerInteraction.Ignore))
                return false;

            Transform root = source.HighlightOcclusionRoot != null
                ? source.HighlightOcclusionRoot
                : entry.Transform;
            Transform hitTransform = hit.transform;
            return hitTransform != null && hitTransform != root && !hitTransform.IsChildOf(root);
        }

        private bool ShowOrUpdateEntry(HighlightEntry entry, Vector3 screenPosition, float distanceRatio)
        {
            if (!entry.IsHighlighted)
            {
                entry.UI = AcquireUI(entry.Owner.HighlightUIPrefabName);
                if (entry.UI == null)
                    return false;

                entry.IsHighlighted = true;
                ActiveHighlightCount++;
                entry.Owner.OnHighlightStateChanged(true);
            }

            entry.UI.ApplyVisual(screenPosition, distanceRatio);
            return true;
        }

        private void EvaluateAimTarget(HighlightEntry entry, Vector2 aimPoint)
        {
            if (entry.UI == null || !entry.UI.TryGetAimScore(aimPoint, out float aimScore))
                return;

            int priority = entry.Owner.InteractionPriority;
            bool isBetter = _bestTarget == null || aimScore < _bestAimScore;
            if (!isBetter && Mathf.Approximately(aimScore, _bestAimScore))
            {
                isBetter = priority > _bestPriority ||
                           priority == _bestPriority && entry.RegistrationOrder < _bestRegistrationOrder;
            }

            if (!isBetter)
                return;

            _bestTarget = entry.Owner;
            _bestTargetBehaviour = entry.OwnerBehaviour;
            _bestAimScore = aimScore;
            _bestPriority = priority;
            _bestRegistrationOrder = entry.RegistrationOrder;
        }

        private void HideEntry(HighlightEntry entry, bool notifyOwner)
        {
            if (!entry.IsHighlighted)
                return;

            entry.IsHighlighted = false;
            ActiveHighlightCount = Mathf.Max(0, ActiveHighlightCount - 1);
            IHighlightUI ui = entry.UI;
            entry.UI = null;
            ReleaseUI(entry.Owner.HighlightUIPrefabName, ui);

            if (notifyOwner && entry.OwnerBehaviour != null)
                entry.Owner.OnHighlightStateChanged(false);
        }

        private void HideAllHighlights()
        {
            ResetBestTarget();
            if (ActiveHighlightCount == 0)
                return;

            for (int i = 0; i < _entries.Count; i++)
                HideEntry(_entries[i], true);
            ActiveHighlightCount = 0;
        }

        private void RemoveEntryAt(int index, bool notifyOwner)
        {
            HighlightEntry removed = _entries[index];
            HideEntry(removed, notifyOwner);
            _entryIndices.Remove(removed.Owner);

            int lastIndex = _entries.Count - 1;
            if (index != lastIndex)
            {
                HighlightEntry moved = _entries[lastIndex];
                _entries[index] = moved;
                _entryIndices[moved.Owner] = index;
            }

            _entries.RemoveAt(lastIndex);

            if (ReferenceEquals(_bestTarget, removed.Owner))
                ResetBestTarget();
        }

        private HighlightUIPool GetOrCreatePool(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName))
                return null;

            if (!_uiPools.TryGetValue(prefabName, out HighlightUIPool pool))
            {
                pool = new HighlightUIPool();
                _uiPools.Add(prefabName, pool);
            }

            return pool;
        }

        private IHighlightUI AcquireUI(string prefabName)
        {
            HighlightUIPool pool = GetOrCreatePool(prefabName);
            if (pool == null)
                return null;

            while (pool.Available.Count > 0)
            {
                IHighlightUI candidate = pool.Available.Pop();
                if (candidate is UIBase uiBase && uiBase != null)
                {
                    uiBase.OnShow();
                    return candidate;
                }

                pool.TotalCount--;
            }

            return CreateUI(prefabName, pool);
        }

        private IHighlightUI CreateUI(string prefabName, HighlightUIPool pool)
        {
            if (!EnsureHighlightCanvas())
                return null;

            UIBase uiBase = UIManager.Instance.CreateUIInstance(prefabName, parent: _highlightCanvas.transform);
            if (uiBase is not IHighlightUI highlightUI)
            {
                if (uiBase != null)
                    Destroy(uiBase.gameObject);
                return null;
            }

            pool.TotalCount++;
            return highlightUI;
        }

        private void ReleaseUI(string prefabName, IHighlightUI ui)
        {
            if (ui is not UIBase uiBase || uiBase == null)
                return;

            uiBase.OnHide();
            HighlightUIPool pool = GetOrCreatePool(prefabName);
            if (pool != null && pool.Available.Count < MaxRetainedUI)
            {
                pool.Available.Push(ui);
                return;
            }

            if (pool != null)
                pool.TotalCount--;
            Destroy(uiBase.gameObject);
        }

        private bool EnsureHighlightCanvas()
        {
            if (_highlightCanvas != null)
                return true;

            UIManager uiManager = UIManager.Instance;
            if (uiManager == null || !uiManager.TryGetMainCanvas(out Canvas mainCanvas))
                return false;

            GameObject canvasObject = new GameObject("HighlightCanvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.layer = mainCanvas.gameObject.layer;
            RectTransform rectTransform = canvasObject.GetComponent<RectTransform>();
            rectTransform.SetParent(mainCanvas.transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            _highlightCanvas = canvasObject.GetComponent<Canvas>();
            _highlightCanvas.overrideSorting = true;
            _highlightCanvas.sortingLayerID = mainCanvas.sortingLayerID;
            _highlightCanvas.sortingOrder = mainCanvas.sortingOrder + 1;
            return true;
        }

        private void StartPrewarmIfNeeded()
        {
            if (Application.isPlaying && _prewarmCoroutine == null && isActiveAndEnabled)
                _prewarmCoroutine = StartCoroutine(PrewarmPools());
        }

        private IEnumerator PrewarmPools()
        {
            while (true)
            {
                if (!EnsureHighlightCanvas())
                {
                    yield return null;
                    continue;
                }

                int createdThisFrame = 0;
                bool hasPendingPool = false;
                foreach (KeyValuePair<string, HighlightUIPool> pair in _uiPools)
                {
                    HighlightUIPool pool = pair.Value;
                    if (pool.PrewarmDisabled)
                        continue;

                    while (pool.TotalCount < PrewarmCount && createdThisFrame < PrewarmPerFrame)
                    {
                        hasPendingPool = true;
                        IHighlightUI ui = CreateUI(pair.Key, pool);
                        if (ui == null)
                        {
                            pool.PrewarmDisabled = true;
                            break;
                        }

                        ReleaseUI(pair.Key, ui);
                        createdThisFrame++;
                    }

                    if (!pool.PrewarmDisabled && pool.TotalCount < PrewarmCount)
                        hasPendingPool = true;
                    if (createdThisFrame >= PrewarmPerFrame)
                        break;
                }

                if (!hasPendingPool)
                    break;

                yield return null;
            }

            _prewarmCoroutine = null;
        }

        private void OnGameplayStatusChanged(EGameplayStatus status)
        {
            if (status != EGameplayStatus.BulletTime)
                HideAllHighlights();
        }

        private void HandleBeforeSceneLoad()
        {
            if (_prewarmCoroutine != null)
            {
                StopCoroutine(_prewarmCoroutine);
                _prewarmCoroutine = null;
            }

            HideAllHighlights();
            _entries.Clear();
            _entryIndices.Clear();

            foreach (HighlightUIPool pool in _uiPools.Values)
            {
                while (pool.Available.Count > 0)
                {
                    if (pool.Available.Pop() is UIBase uiBase && uiBase != null)
                        Destroy(uiBase.gameObject);
                }
            }

            _uiPools.Clear();
            if (_highlightCanvas != null)
                Destroy(_highlightCanvas.gameObject);
            _highlightCanvas = null;
            ResetBestTarget();
        }

        private void ResetBestTarget()
        {
            _bestTarget = null;
            _bestTargetBehaviour = null;
            _bestAimScore = float.PositiveInfinity;
            _bestPriority = int.MinValue;
            _bestRegistrationOrder = ulong.MaxValue;
        }
    }
}
