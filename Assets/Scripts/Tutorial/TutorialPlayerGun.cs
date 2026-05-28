using System.Collections;
using Player;
using UI;
using UnityEngine;

namespace Tutorial
{
    public class TutorialPlayerGun : PlayerGun, ITutorialContext
    {
        public TutorialDirector Director => director;
        [SerializeField] private TutorialDirector director;
        public string ContextID => contextID;
        [SerializeField] private string contextID = "PlayerAttack";
        
        private bool _isFeatureUnlocked;
        private bool _isCompleted;
        
        private void Awake()
        {
            director?.RegisterContext(this);
        }
        
        public void Enter()
        {
            _isFeatureUnlocked = true;
            UIManager.Instance.ShowUI("TPlayerAttackUI");
        }

        public void Exit()
        {
            UIManager.Instance.HideUI("TPlayerAttackUI");
        }

        public IEnumerator NextStepBuffer()
        {
            UIManager.Instance.GetCurrentUI("TPlayerAttackUI")?.UpdateUI(true);
            yield return new WaitForSecondsRealtime(3f);
            Director.NextStep();
        }

        protected override void OnAttack()
        {
            if (!_isFeatureUnlocked) return;
            if (!_isCompleted)
            {
                StartCoroutine(NextStepBuffer());
                _isCompleted = true;
            }
            base.OnAttack();
        }
    }
}
