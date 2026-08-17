#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using CharacterUniversal;
using NUnit.Framework;
using UI;
using UnityEngine;
using UnityEngine.TestTools;

public class HighlightManagerPlayModeIntegrationTests
{
    private sealed class TestHighlightTarget : MonoBehaviour, IHighlightInViewport, IHighlightOcclusionSource
    {
        public bool eligible = true;
        public bool useOcclusion;
        public int priority;
        public Transform HighlightTransform => transform;
        public string HighlightUIPrefabName => "HighlightRing";
        public float HighlightMinDistance => 0f;
        public float HighlightMaxDistance => 10f;
        public bool IsHighlightEligible => eligible;
        public int InteractionPriority => priority;
        public bool UseHighlightOcclusion => useOcclusion;
        public LayerMask HighlightOcclusionMask => 0;
        public float HighlightOcclusionInterval => 0.01f;
        public Transform HighlightOcclusionRoot => transform;
        public int stateChangeCount;

        public void OnHighlightStateChanged(bool isHighlighted)
        {
            stateChangeCount++;
        }
    }

    private readonly List<GameObject> _createdObjects = new List<GameObject>();
    private readonly List<TestHighlightTarget> _targets = new List<TestHighlightTarget>();
    private HighlightManager _manager;
    private GameplayManager _gameplayManager;
    private UIManager _uiManager;
    private Canvas _canvas;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        yield return new EnterPlayMode();

        _gameplayManager = GameplayManager.Instance;
        _uiManager = UIManager.Instance;
        _manager = HighlightManager.Instance;

        GameObject canvasObject = CreateObject("TestMainCanvas", typeof(RectTransform), typeof(Canvas));
        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _uiManager.RegisterMainCanvas(_canvas);

        GameObject cameraObject = CreateObject("TestCamera", typeof(Camera));
        Camera camera = cameraObject.GetComponent<Camera>();
        cameraObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        GameObject playerObject = CreateObject("TestPlayer", typeof(Player.Player));
        Player.Player player = playerObject.GetComponent<Player.Player>();
        player.playerCamera = camera;
        _gameplayManager.Player = player;
        _gameplayManager.SetGameplayStatus(EGameplayStatus.BulletTime, true);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        for (int i = 0; i < _targets.Count; i++)
            _manager.Unregister(_targets[i]);
        _targets.Clear();

        _gameplayManager.SetGameplayStatus(EGameplayStatus.Default, true);
        _gameplayManager.Player = null;
        _uiManager.UnregisterMainCanvas(_canvas);

        for (int i = _createdObjects.Count - 1; i >= 0; i--)
        {
            if (_createdObjects[i] != null)
                Object.Destroy(_createdObjects[i]);
        }
        _createdObjects.Clear();
        yield return null;
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator Register_IsIdempotent_AndSwapRemovalKeepsIndicesValid()
    {
        int baseline = _manager.RegisteredCount;
        TestHighlightTarget first = CreateTarget(new Vector3(-0.5f, 0f, 5f));
        TestHighlightTarget middle = CreateTarget(new Vector3(0f, 0f, 5f));
        TestHighlightTarget last = CreateTarget(new Vector3(0.5f, 0f, 5f));

        _manager.Register(first);
        _manager.Register(first);
        _manager.Register(middle);
        _manager.Register(last);
        Assert.That(_manager.RegisteredCount, Is.EqualTo(baseline + 3));

        _manager.Unregister(middle);
        _manager.Unregister(last);
        Assert.That(_manager.RegisteredCount, Is.EqualTo(baseline + 1));

        _manager.Unregister(first);
        Assert.That(_manager.RegisteredCount, Is.EqualTo(baseline));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Stress200Targets_CapsOcclusionChecks_AndSelectsOneBestTarget()
    {
        int baselineActive = _manager.ActiveHighlightCount;
        for (int i = 0; i < 200; i++)
        {
            float x = (i % 10 - 4.5f) * 0.08f;
            float y = (i / 10 % 10 - 4.5f) * 0.08f;
            float z = i < 100 ? 5f : 20f;
            TestHighlightTarget target = CreateTarget(new Vector3(x, y, z));
            target.useOcclusion = i < 100;
            target.priority = i == 44 ? 1 : 0;
            _manager.Register(target);
        }

        for (int frame = 0; frame < 15; frame++)
        {
            yield return null;
            Assert.That(_manager.LastFrameOcclusionChecks, Is.LessThanOrEqualTo(20));
        }

        Assert.That(_manager.ActiveHighlightCount, Is.EqualTo(baselineActive + 100));
        Assert.That(_manager.TryGetBestTarget(out IHighlightInViewport bestTarget), Is.True);
        Assert.That(bestTarget, Is.Not.Null);
    }

    [UnityTest]
    public IEnumerator ReenteringRange_ReusesReleasedHighlightUI()
    {
        TestHighlightTarget target = CreateTarget(new Vector3(0f, 0f, 5f));
        _manager.Register(target);

        for (int i = 0; i < 12; i++)
            yield return null;

        Transform highlightCanvas = _canvas.transform.Find("HighlightCanvas");
        Assert.That(highlightCanvas, Is.Not.Null);
        Transform activeRing = FindActiveChild(highlightCanvas);
        Assert.That(activeRing, Is.Not.Null);
        int firstInstanceId = activeRing.gameObject.GetInstanceID();

        target.eligible = false;
        yield return null;
        target.eligible = true;
        yield return null;

        Transform reusedRing = FindActiveChild(highlightCanvas);
        Assert.That(reusedRing, Is.Not.Null);
        Assert.That(reusedRing.gameObject.GetInstanceID(), Is.EqualTo(firstInstanceId));
        Assert.That(target.stateChangeCount, Is.EqualTo(3));
    }

    private TestHighlightTarget CreateTarget(Vector3 position)
    {
        GameObject targetObject = CreateObject("HighlightTarget");
        targetObject.transform.position = position;
        TestHighlightTarget target = targetObject.AddComponent<TestHighlightTarget>();
        _targets.Add(target);
        return target;
    }

    private GameObject CreateObject(string name, params System.Type[] components)
    {
        GameObject gameObject = components.Length == 0
            ? new GameObject(name)
            : new GameObject(name, components);
        _createdObjects.Add(gameObject);
        return gameObject;
    }

    private static Transform FindActiveChild(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.gameObject.activeSelf)
                return child;
        }
        return null;
    }
}
#endif
