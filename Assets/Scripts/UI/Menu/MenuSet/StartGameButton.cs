using UnityEngine;

namespace UI.Menu.MenuSet
{
    public class StartGameButton : MonoBehaviour
    {
        public void StartGame()
        {
            LevelManager.Instance.LoadNextScene();
        }
    }
}
