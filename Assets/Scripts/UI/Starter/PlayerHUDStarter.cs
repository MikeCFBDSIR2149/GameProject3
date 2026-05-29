using UI;
using UnityEngine;

namespace Starter
{
    public class PlayerHUDStarter : MonoBehaviour
    {
        private void Start()
        {
            UIManager.Instance.ShowUI("HealthUI");
			UIManager.Instance.ShowUI("EnergyUI");
        }
    }
}
