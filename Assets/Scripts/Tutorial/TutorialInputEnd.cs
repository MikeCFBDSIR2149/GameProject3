using System;
using System.Collections;
using UI;
using UnityEngine;

namespace Tutorial
{
    public class TutorialInputEnd : MonoBehaviour, ITutorialContext
    {
        public TutorialDirector Director => director;
        [SerializeField] private TutorialDirector director;
        public string ContextID => contextID;
        [SerializeField] private string contextID;

        private System.Action _onInputAction;

        private void Awake()
        {
            Director.RegisterContext(this);
        }

        public void Enter()
        {
            _onInputAction = () => StartCoroutine(NextStepBuffer());
            GlobalInputController.Instance.OnSubmitInputChanged += _onInputAction;
            UIManager.Instance.ShowUI("TInputEndUI");
        }

        public void Exit()
        {
            GlobalInputController.Instance.OnSubmitInputChanged -= _onInputAction;
            UIManager.Instance.HideUI("TInputEndUI");
        }

        public IEnumerator NextStepBuffer()
        {
            yield return new WaitForSecondsRealtime(1f);
            Director.NextStep();
        }
    }
}
