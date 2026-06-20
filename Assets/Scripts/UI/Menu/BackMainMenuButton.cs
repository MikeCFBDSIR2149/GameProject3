using UnityEngine;

namespace UI.Menu
{
    public class BackMainMenuButton : MonoBehaviour
    {
        public void BackMainMenu()
        {
            GameplayManager.Instance.SetDoNotTriggerListener(true);
            LevelManager.Instance.LoadScene(0);
        }
    }
}
