using CharacterUniversal;
using UnityEngine;

namespace Enemy
{
    public class SniperEnemy : EnemyController
    {
        [Header("Sniper")]
        public float shootDistance = 15f;

        [Tooltip("锁定预警时长：激光连续指到玩家这么久，才会开枪")]
        public float aimDuration = 3f;

        [Tooltip("开完一枪后的冷却时间（避免瞬间又开始下一次3秒锁定）")]
        public float shootCooldown = 2f;

        public Transform firePoint;

        [Tooltip("对象池 key（需与 ObjectPool.poolKey 一致）")]
        public string bulletPoolKey = "EnemyBullet";

        [Tooltip("狙击子弹速度更快")]
        public float bulletSpeed = 40f;

        [Header("Laser")]
        [Tooltip("用于显示红色瞄准线的 LineRenderer（建议在Prefab上配好：细、红、简单材质）")]
        public LineRenderer laser;

        [Tooltip("激光最大长度")]
        public float laserMaxDistance = 100f;

        [Tooltip("哪些层算作遮挡物（墙体/地形等）。建议不包含 Player 层。")]
        public LayerMask obstacleMask;

        [Tooltip("LineRenderer 宽度")]
        public float laserWidth = 0.02f;

        [Tooltip("激光颜色")]
        public Color laserColor = Color.red;

        [Header("Weapon Aim")]
        [Tooltip("枪械要单独对准玩家的根节点。建议拖狙击枪模型的父物体，而不是 firePoint。")]
        [SerializeField] private Transform weaponAimRoot;

        [Tooltip("如果枪模型默认朝向不是 Z+，可以在这里修正角度")]
        [SerializeField] private Vector3 weaponAimOffsetEuler = Vector3.zero;

        [Header("Health Fallback")]
        [Tooltip("兜底查找 Health。父类没直接读到时，这里仍可让敌人正常死亡。")]
        [SerializeField] private bool searchHealthInChildren = true;

        private float _aimTimer;
        private float _cooldownTimer;
        private bool _isPaused;

        private Health _localHealth;

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnStatusChanged;

            CacheLocalHealth();
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnStatusChanged;

            SetLaserVisible(false);
            _aimTimer = 0f;
            _cooldownTimer = 0f;
        }

        protected override void Start()
        {
            // 保留 EnemyController.Start() 初始化 player / agent / patrol / animator
            base.Start();

            CacheLocalHealth();
            SetupLaserIfNeeded();
            SetLaserVisible(false);
        }

        private void CacheLocalHealth()
        {
            if (_localHealth != null) return;

            if (!TryGetComponent(out _localHealth) && searchHealthInChildren)
            {
                _localHealth = GetComponentInChildren<Health>(true);
            }
        }

        private void SetupLaserIfNeeded()
        {
            if (laser == null) return;

            laser.positionCount = 2;
            laser.startWidth = laserWidth;
            laser.endWidth = laserWidth;
            laser.startColor = laserColor;
            laser.endColor = laserColor;
            laser.enabled = false;
        }

        private void OnStatusChanged(EGameplayStatus status)
        {
            _isPaused = GameplayManager.Instance != null && !GameplayManager.Instance.CanPerformGameplayActions;

            if (_isPaused && agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            else if (!_isPaused && agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = false;
            }

            if (_isPaused)
                SetLaserVisible(false);
        }

        protected override void Update()
        {
            // 先检查死亡，保证血量归零一定会执行死亡动画 / 消失
            if (CheckAndHandleDeath())
            {
                SetLaserVisible(false);
                return;
            }

            // 父类没读到 Health 时，这里再兜底一次
            if (_localHealth != null && _localHealth.CurrentHealth <= 0f)
            {
                SetLaserVisible(false);
                Die();
                return;
            }

            if (_isPaused) return;

            bool playerInDetectRange = (player != null && Vector3.Distance(transform.position, player.position) < detectRange);

            if (playerInDetectRange)
            {
                OnPlayerDetected();
            }
            else
            {
                // 玩家离开范围后，重新锁定前的状态必须清掉，避免激光残留
                ResetLockAndLaser();
                Patrol();
            }
        }

        protected override void OnPlayerDetected()
        {
            if (_isPaused) return;
            if (player == null) return;
            if (firePoint == null)
            {
                Debug.LogWarning("[SniperEnemy] firePoint is null!");
                return;
            }

            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > shootDistance)
            {
                // 远：追击
                SetLaserVisible(false);
                _aimTimer = 0f;

                TrySetDestination(player.position);
                return;
            }

            // 近：停下，开始狙击
            StopAgentMovement();

            // 敌人本体朝向玩家
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            // 枪械单独对准玩家
            AimWeaponAtPlayer();

            // 冷却中：不显示激光，重新开始锁定
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                SetLaserVisible(false);
                _aimTimer = 0f;
                return;
            }

            // 更新激光并判断是否视野畅通
            bool hasClearShotToPlayer = UpdateLaserAndCheckClearShot();

            if (hasClearShotToPlayer)
            {
                _aimTimer += Time.deltaTime;

                if (_aimTimer >= aimDuration)
                {
                    // 开枪前最后确认一次，防止临界帧穿墙
                    if (UpdateLaserAndCheckClearShot())
                    {
                        // 开枪前再对准一次，减少模型偏差
                        AimWeaponAtPlayer();

                        Shoot();
                        _cooldownTimer = shootCooldown;
                    }

                    _aimTimer = 0f;
                }
            }
            else
            {
                // 视线被挡住 / 没指到玩家：锁定进度归零
                _aimTimer = 0f;
            }
        }

        private void ResetLockAndLaser()
        {
            SetLaserVisible(false);
            _aimTimer = 0f;

            // 如果你希望玩家离开范围后，连冷却也重置，就保留这一句
            _cooldownTimer = 0f;
        }

        private void StopAgentMovement()
        {
            if (agent == null) return;
            if (!agent.isActiveAndEnabled) return;
            if (!agent.isOnNavMesh) return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        /// <summary>
        /// 枪械单独朝向玩家
        /// </summary>
        private void AimWeaponAtPlayer()
        {
            if (weaponAimRoot == null || player == null) return;

            Vector3 dir = player.position - weaponAimRoot.position;
            if (dir.sqrMagnitude < 0.0001f) return;

            weaponAimRoot.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(weaponAimOffsetEuler);
        }

        /// <summary>
        /// 画激光并判断：从 firePoint 朝 player 的方向 Raycast，是否首先命中 Player
        /// </summary>
        private bool UpdateLaserAndCheckClearShot()
        {
            if (laser == null) return CheckClearShotOnly();
            SetLaserVisible(true);

            Vector3 origin = firePoint.position;
            Vector3 dir = (player.position - origin).normalized;

            float distToPlayer = Vector3.Distance(origin, player.position);

            bool blocked = Physics.Raycast(origin, dir, out RaycastHit hitObstacle, distToPlayer, obstacleMask, QueryTriggerInteraction.Ignore);

            if (blocked)
            {
                laser.SetPosition(0, origin);
                laser.SetPosition(1, hitObstacle.point);
                return false;
            }

            Vector3 end = player.position;
            laser.SetPosition(0, origin);
            laser.SetPosition(1, end);

            return true;
        }

        /// <summary>
        /// 如果没配 laser，也能正常判断是否被障碍遮挡
        /// </summary>
        private bool CheckClearShotOnly()
        {
            if (firePoint == null || player == null) return false;

            Vector3 origin = firePoint.position;
            Vector3 dir = (player.position - origin).normalized;
            float distToPlayer = Vector3.Distance(origin, player.position);

            return !Physics.Raycast(origin, dir, distToPlayer, obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private void SetLaserVisible(bool visible)
        {
            if (laser != null) laser.enabled = visible;
        }

        private void Shoot()
        {
            if (firePoint == null)
            {
                Debug.LogWarning("[SniperEnemy] firePoint is null!");
                return;
            }

            TriggerShootAnim();

            GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, firePoint.position, Quaternion.identity);
            if (bullet == null) return;

            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript == null) return;

            Vector3 dir = (player.position - firePoint.position).normalized;
            bulletScript.Init(dir * bulletSpeed);
            bulletScript.SetSender(this);
        }
    }
}