using UI;
using UnityEngine;

namespace Test
{
    public class AdministratorUIInvoker : MonoBehaviour
    {
        private void OnEnable()
        {
            if (GlobalInputController.Instance != null)
                GlobalInputController.Instance.OnReservedKeyInputChanged += OnReservedKeyInputChanged;
        }

        private void OnDisable()
        {
            if (GlobalInputController.Instance != null)
                GlobalInputController.Instance.OnReservedKeyInputChanged -= OnReservedKeyInputChanged;
        }
        
        private void OnReservedKeyInputChanged()
        {
            UIManager.Instance.ShowUI("AdministratorPanel");
        }
    }
}
