using UnityEngine;

namespace UI.Menu
{
    public class NextLevelButton : MonoBehaviour
    {
        public void NextLevel()
        {
            if (!LevelManager.Instance.LoadNextScene()) LevelManager.Instance.LoadScene(0);
        }
    }
}
