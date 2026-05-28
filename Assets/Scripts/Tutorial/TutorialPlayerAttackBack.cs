using System.Collections;
using CharacterUniversal;
using UnityEngine;
using Player;
using UI;

namespace Tutorial
{
    public class TutorialPlayerAttackBack : PlayerAttackBack, ITutorialContext
    {
        public TutorialDirector Director => director;
        [SerializeField] private TutorialDirector director;
        public string ContextID => contextID;
        [SerializeField] private string contextID = "PlayerAttackBack";
        
        private bool _isFeatureUnlocked;
        private bool _isCompleted;

        private void Awake()
        {
            director?.RegisterContext(this);
        }

        public void Enter()
        {
            _isFeatureUnlocked = true;
            UIManager.Instance.ShowUI("TPlayerAttackBackUI");
        }

        public void Exit()
        {
            UIManager.Instance.HideUI("TPlayerAttackBackUI");
        }

        public IEnumerator NextStepBuffer()
        {
            UIManager.Instance.GetCurrentUI("TPlayerAttackBackUI")?.UpdateUI(true);
            yield return new WaitForSecondsRealtime(3f);
            Director.NextStep();
        }

        public override void RegisterBulletReturn(ISender sender, Vector3 spawnPosition)
        {
            if (!_isFeatureUnlocked) return;
            if (!_isCompleted)
            {
                StartCoroutine(NextStepBuffer());
                _isCompleted = true;
            }
            base.RegisterBulletReturn(sender, spawnPosition);
        }
    }
}



