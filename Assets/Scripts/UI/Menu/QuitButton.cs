using UnityEngine;

namespace UI.Menu
{
    public class QuitButton : MonoBehaviour
    {
        public void QuitGame()
        {
            GameplayManager.Instance.SetDoNotTriggerListener(true);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
