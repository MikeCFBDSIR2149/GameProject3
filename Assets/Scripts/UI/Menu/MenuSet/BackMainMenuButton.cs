using UnityEngine;

namespace UI.Menu.MenuSet
{
    public class BackMainMenuButton : MonoBehaviour
    {
        public void BackMainMenu()
        {
            LevelManager.Instance.LoadScene(0);
        }
    }
}
