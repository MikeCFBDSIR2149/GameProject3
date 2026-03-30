using UnityEngine;
using UserOptions;

namespace UI.Menu.MenuSet
{
    public class UserOptionsButton : MonoBehaviour
    {
        private static bool isOpen;

        public void OpenUserOptionsPanel()
        {
            if (isOpen) return;
            UIManager.Instance.ShowUI("UserOptionsMain", asRootCanvas: true);
            isOpen = true;
        }

        public void CloseUserOptionsPanel()
        {
            if (!isOpen) return;
            OptionsManager.Instance.SaveOptions();
            UIManager.Instance.HideUI("UserOptionsMain");
            isOpen = false;
        }
    }
}
