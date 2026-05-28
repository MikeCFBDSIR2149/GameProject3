using UnityEngine;
using Player;

namespace Tutorial
{
    public class TutorialPlayerMotor : PlayerMotor, ITutorialContext
    {
        private TutorialDirector _tutorialDirector;

        private bool _isFeatureUnlocked;

        private bool _isTutorialActive;

        // ITutorialContext 标识
        [SerializeField] private string _contextID = "PlayerMotor";
        public string ContextID => _contextID;

        // 当步骤开始时调用：同时解锁功能并标记步骤激活
        public void Enter()
        {
            _isFeatureUnlocked = true;
            _isTutorialActive = true;
        }

        // 当步骤结束时调用：保持功能解锁，但标记步骤不再激活
        public void Exit()
        {
            _isTutorialActive = false;
        }

        // 从 TutorialDirector 注入引用（由 TutorialDirector 在初始化时调用）
        public void SetDirector(TutorialDirector director)
        {
            _tutorialDirector = director;
        }

        // 重写 FixedUpdate：按照提示要求实现拦截逻辑，并保持与基类 FixedUpdate 的调用关系
        protected override void FixedUpdate()
        {
            if (!_isFeatureUnlocked)
            {
                // 功能未解锁，拦截行为
                return;
            }

            // 执行基类的 FixedUpdate 中的移动逻辑
            base.FixedUpdate();

            // 在教程进行时，检查 WASD 是否被按下（任意一个键）
            if (_isTutorialActive)
            {
                if (Input.GetKeyDown(KeyCode.W) ||
                    Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.S) ||
                    Input.GetKeyDown(KeyCode.D))
                {
                    // 使用注入的 Director 引用推进到下一步骤
                    _tutorialDirector?.NextStep();
                }
            }
        }
    }
}

