using UnityEngine;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public Camera playerCamera;

        private void Awake()
        {
            // 注册自身到GameplayManager
            if (GameplayManager.Instance != null)
            {
                GameplayManager.Instance.Player = this;
            }
            else
            {
                Debug.LogError("GameplayManager.Instance is null, Player注册失败");
            }
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public Camera GetPlayerCamera()
        {
            return playerCamera;
        }
    }
}

