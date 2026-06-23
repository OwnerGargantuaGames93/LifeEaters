using Cinemachine;
using UnityEngine;

namespace Boundary.Camera
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake instance;

        [SerializeField] private float globalShakeForce = 1f;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            } 
        }

        public void ExecCameraShake(CinemachineImpulseSource impulseSource)
        {
            impulseSource.GenerateImpulseWithForce(globalShakeForce);
        }
    }
}
