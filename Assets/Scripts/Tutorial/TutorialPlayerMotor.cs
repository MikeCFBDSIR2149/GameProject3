using UnityEngine;
using Player;
using UI;

namespace Tutorial
{
    public class TutorialPlayerMotor : PlayerMotor, ITutorialContext
    {
        public TutorialDirector Director => director;
        [SerializeField] private TutorialDirector director;
        public string ContextID => contextID;
        [SerializeField] private string contextID = "PlayerMotor";
        
        private bool _isFeatureUnlocked;

        protected override void Awake()
        {
            director?.RegisterContext(this);
            base.Awake();
        }

        public void Enter()
        {
            _isFeatureUnlocked = true;
            UIManager.Instance.ShowUI("TPlayerMotorUI");
        }

        public void Exit()
        {
            UIManager.Instance.HideUI("TPlayerMotorUI");
        }

        protected override void SetMoveInput(Vector2 input)
        {
            if (input != Vector2.zero)
            {
                Director.NextStep();
            }
            base.SetMoveInput(input);
        }
    }
}

