using UnityEngine;

namespace UI.Menu
{
    public class BackMainMenuButton : MonoBehaviour
    {
        public void BackMainMenu()
        {
            LevelManager.Instance.LoadScene(0);
        }
    }
}
