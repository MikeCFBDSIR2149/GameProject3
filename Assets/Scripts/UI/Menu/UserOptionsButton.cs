using UnityEngine;
using UserOptions;

namespace UI.Menu
{
    public class UserOptionsButton : MonoBehaviour
    {
        private static bool _isOpen;

        public static void ResetOpenState()
        {
            _isOpen = false;
        }

        public void OpenUserOptionsPanel()
        {
            if (_isOpen) return;
            UIManager.Instance.ShowUI("UserOptionsMain", asRootCanvas: true);
            _isOpen = true;
        }

        public void CloseUserOptionsPanel()
        {
            if (!_isOpen) return;
            OptionsManager.Instance.SaveOptions();
            UIManager.Instance.HideUI("UserOptionsMain");
            _isOpen = false;
        }
    }
}
