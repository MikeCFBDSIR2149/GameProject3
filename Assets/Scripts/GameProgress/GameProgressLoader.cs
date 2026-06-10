using System.Collections;
using UI;
using UnityEngine;

namespace GameProgress
{
    public class GameProgressLoader : MonoBehaviour, ISyncFromGameProgress
    {
        private IEnumerator Start()
        {
            yield return new WaitForSecondsRealtime(1f);
            SyncFromGameProgress();
        }

        public void SyncFromGameProgress()
        {
            GameProgressManager progressManager = GameProgressManager.Instance;
            if (progressManager == null)
            {
                Debug.LogWarning("[GameProgressLoader] GameProgressManager.Instance is null.");
                return;
            }

            if (progressManager.IsNewPlayer())
            {
                UIManager.Instance.ShowUI("StarterTipPanel");
            }
            else
            {
                Debug.Log("[GameProgressLoader] 老手");
            }
        }

    }
}
