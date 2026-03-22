using Unity.Cinemachine;
using UnityEngine;

namespace Player
{
    public class PlayerCameraDetector : MonoBehaviour
    {
        public CinemachineThirdPersonAim cameraAim;

        public Vector3 DetectAimPosition()
        {
            return cameraAim.AimTarget;
        }
    }
}
