using UnityEngine;

namespace UI.Menu
{
    public class StartGameButton : MonoBehaviour
    {
        public void StartGame()
        {
            // LevelManager.Instance.LoadNextScene();
            UIManager.Instance.ShowUI("LevelSelectionPanel");
        }
    }
}
