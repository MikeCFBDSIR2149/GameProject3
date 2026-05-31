using System.Collections;
using UnityEngine;
using Player;
using UI;

namespace Tutorial
{
    public class TutorialPlayerBulletTimeSkill : PlayerBulletTimeSkill, ITutorialContext
    {
        [Header("Tutorial Context Settings")]
        public TutorialDirector Director => director;
        [SerializeField] private TutorialDirector director;
        public string ContextID => contextID;
        [SerializeField] private string contextID = "PlayerBulletTimeSkill";
        
        private bool _isFeatureUnlocked;
        private (bool, bool) _isCompleted;

        private void Awake()
        {
            director?.RegisterContext(this);
        }

        public void Enter()
        {
            _isFeatureUnlocked = true;
            UIManager.Instance.ShowUI("TPlayerBulletTimeSkillUI");
        }

        public void Exit()
        {
            UIManager.Instance.HideUI("TPlayerBulletTimeSkillUI");
        }

        public IEnumerator NextStepBuffer()
        {
            yield return new WaitForSecondsRealtime(3f);
            Director.NextStep();
        }

        protected override void TryActivateBulletTime()
        {
            if (!_isFeatureUnlocked)
                return;

            if (_isCompleted != (true, true))
            {
                if (!_isBulletTimeActive)
                {
                    _isCompleted.Item1 = true;
                }
                else
                {
                    _isCompleted.Item2 = true;
                    if (_isCompleted == (true, true)) StartCoroutine(NextStepBuffer());
                }
                UIManager.Instance.GetCurrentUI("TPlayerBulletTimeSkillUI")?.UpdateUI(_isCompleted);
            }

            base.TryActivateBulletTime();
        }
    }
}



