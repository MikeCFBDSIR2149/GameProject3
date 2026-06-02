using System.Collections.Generic;
using UnityEngine;

namespace UI.Cutscene
{
    /// <summary>
    /// Manages an ordered, serialized list of CutsceneStep objects. Only one
    /// step is active at a time. Listens to GlobalInputController's submit event
    /// (OnSubmitInputChanged) to advance to the next step. When finished the
    /// loader deactivates its GameObject.
    /// </summary>
    public class CutsceneLoader : MonoBehaviour
    {
        [SerializeField] private List<CutsceneStep> steps = new List<CutsceneStep>();

        private int _currentIndex = -1;

        private void OnEnable()
        {
            // subscribe to the project's GlobalInputController (user provided)
            if (GlobalInputController.Instance != null)
                GlobalInputController.Instance.OnSubmitInputChanged += OnAdvanceRequested;

            StartCutscene();
        }

        private void OnDisable()
        {
            if (GlobalInputController.Instance != null)
                GlobalInputController.Instance.OnSubmitInputChanged -= OnAdvanceRequested;
        }

        private void StartCutscene()
        {
            // Disable game input during cutscene
            if (GlobalInputController.Instance != null)
                GlobalInputController.Instance.DisableInputControllers();

            // ensure all steps start inactive
            foreach (var s in steps)
            {
                if (s != null)
                    s.gameObject.SetActive(false);
            }

            _currentIndex = -1;
            NextStep();
        }

        private void OnAdvanceRequested()
        {
            NextStep();
        }

        private void NextStep()
        {
            // exit current
            if (_currentIndex >= 0 && _currentIndex < steps.Count)
            {
                var cur = steps[_currentIndex];
                if (cur != null)
                    cur.Exit();
            }

            _currentIndex++;

            if (_currentIndex >= steps.Count)
            {
                // Re-enable game input when cutscene finishes
                if (GlobalInputController.Instance != null)
                    GlobalInputController.Instance.EnableInputControllers();

                // finished
                gameObject.SetActive(false);
                return;
            }

            var next = steps[_currentIndex];
            if (next != null)
                next.Enter();
        }
    }
}
