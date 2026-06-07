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

        private float _aimTimer = 0f;
        private float _cooldownTimer = 0f;
        private bool _isPaused = false;

        private void OnEnable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged += OnStatusChanged;
        }

        private void OnDisable()
        {
            if (GameplayManager.Instance != null)
                GameplayManager.Instance.OnStatusChanged -= OnStatusChanged;
        }

        protected override void Start()
        {
            // 保留 EnemyController.Start() 初始化 player/agent/patrol
            base.Start();

            SetupLaserIfNeeded();
            SetLaserVisible(false);
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
            // 你也可以在 inspector 里直接设置材质，比如 Unlit/Color 红色
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

            // 暂停时隐藏激光（可选）
            if (_isPaused) SetLaserVisible(false);
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

            // 水平朝向玩家（避免上下点头导致看起来怪）
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            // 冷却中：只显示激光 or 不显示都行，这里选择不显示并清空锁定
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                SetLaserVisible(false);
                _aimTimer = 0f;
                return;
            }

            // 更新激光、并判断是否“无障碍命中玩家”
            bool hasClearShotToPlayer = UpdateLaserAndCheckClearShot();

            if (hasClearShotToPlayer)
            {
                _aimTimer += Time.deltaTime;

                if (_aimTimer >= aimDuration)
                {
                    // 开枪前最后确认一次（防止临界帧穿墙）
                    if (UpdateLaserAndCheckClearShot())
                    {
                        Shoot();
                        _cooldownTimer = shootCooldown;
                    }

                    _aimTimer = 0f;
                }
            }
            else
            {
                // 视线被挡住/没指到玩家：锁定进度归零
                _aimTimer = 0f;
            }
        }
        protected override void Update()
        {
            if (_isPaused) return;

            bool playerInDetectRange = (player != null && Vector3.Distance(transform.position, player.position) < detectRange);

            if (playerInDetectRange)
            {
                OnPlayerDetected();
            }
            else
            {
                // 关键：玩家离开 detectRange 或 player 为 null 时，必须清理狙击状态，否则激光会残留
                ResetLockAndLaser();

                Patrol();
            }
        }

        private void ResetLockAndLaser()
        {
            SetLaserVisible(false);
            _aimTimer = 0f;

            // 如果你希望“玩家脱离视野后，连冷却都需要重新来”，可以也清掉冷却：
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
        /// 画激光并判断：从 firePoint 朝 player 的方向 Raycast，是否首先命中 Player
        /// </summary>
        private bool UpdateLaserAndCheckClearShot()
        {
            if (laser == null) return CheckClearShotOnly();
            SetLaserVisible(true);

            Vector3 origin = firePoint.position;
            Vector3 dir = (player.position - origin).normalized;

            // 关键点：我们希望“碰到障碍物就算挡住”
            // 所以用 Raycast：如果命中障碍物，则激光终点在障碍物点，并返回 false
            // 如果不命中障碍物，则激光拉到玩家方向最大距离 or 到玩家位置，并返回 true（还要确认前方就是玩家）
            //
            // 更严格的方式：RaycastAll 排序，或用两个 Ray：
            // 1) Raycast obstacleMask，看最近障碍点
            // 2) 再判断玩家是否在障碍点之前
            //
            // 这里用“到玩家距离范围内的障碍检测”实现：只要玩家与 origin 之间存在 obstacle，就算遮挡。
            float distToPlayer = Vector3.Distance(origin, player.position);

            bool blocked = Physics.Raycast(origin, dir, out RaycastHit hitObstacle, distToPlayer, obstacleMask, QueryTriggerInteraction.Ignore);

            if (blocked)
            {
                // 激光打在障碍物上
                laser.SetPosition(0, origin);
                laser.SetPosition(1, hitObstacle.point);
                return false;
            }

            // 没被 obstacle 挡住：激光直接指到“玩家方向”
            // 终点你可以用玩家位置（有红点感），也可以打到更远（更像镭射）
            Vector3 end = player.position; // 推荐：直接落在玩家身上
            laser.SetPosition(0, origin);
            laser.SetPosition(1, end);

            // 这里还可以做一个“玩家层命中确认”（可选但更严谨）
            // 因为上面只保证没有障碍物，理论上就是 clear shot
            return true;
        }

        /// <summary>
        /// 如果没配 laser，也能正常判断是否被障碍遮挡
        /// </summary>
        private bool CheckClearShotOnly()
        {
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
            // 开枪时你也可以瞬间闪一下激光/改颜色，这里先保持简单
            GameObject bullet = ObjectPoolManager.Instance.Get(bulletPoolKey, firePoint.position, Quaternion.identity);
            if (bullet == null) return;
            TriggerShootAnim();
            EnemyBullet bulletScript = bullet.GetComponent<EnemyBullet>();
            if (bulletScript == null) return;

            Vector3 dir = (player.position - firePoint.position).normalized;
            bulletScript.Init(dir * bulletSpeed);
            bulletScript.SetSender(this);
        }
    }
}