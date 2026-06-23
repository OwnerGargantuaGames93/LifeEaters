using Boundary.Commands;
using UnityEngine;

namespace Boundary.Pit
{
    public class Pit : MonoBehaviour
    {
        [SerializeField]
        private float pitLifeTime = 7f;

        private bool _isDestroyable;

        private void Update()
        {
            if (UserInput.instance.PitGenerateWasPressedThisFrame() && _isDestroyable)
            {
                Destroy(gameObject);
            }
        }

        public void SetIsDestroyable(bool isDestroyable)
        {
            _isDestroyable = isDestroyable;
            Destroy(gameObject, pitLifeTime);
        }
    }
}
