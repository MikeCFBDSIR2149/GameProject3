using System.Collections;
using UnityEngine;
using Player;

namespace Tutorial
{
    public class TutorialPlayerAttackBack : PlayerAttackBack, ITutorialContext
    {
        public TutorialDirector Director => director;
        [SerializeField] private TutorialDirector director;
        public string ContextID => contextID;
        [SerializeField] private string contextID = "PlayerAttackBack";
        
        private bool _isFeatureUnlocked;

        private void Awake()
        {
            director?.RegisterContext(this);
        }

        public void Enter()
        {
            _isFeatureUnlocked = true;
        }

        public void Exit()
        {
            // 当步骤结束时保持解锁状态（教程结束后玩家应该保留该功能）
            // 如果需要锁定，改为：_isFeatureUnlocked = false;
        }

        public IEnumerator NextStepBuffer()
        {
            throw new System.NotImplementedException();
        }
    }
}



