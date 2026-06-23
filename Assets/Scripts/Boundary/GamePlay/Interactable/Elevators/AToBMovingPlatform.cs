using UnityEngine;

namespace Boundary.Interactable
{
    public class AToBMovingPlatform: MonoBehaviour
    {
        
        [SerializeField] private Transform pointAPosition;
        [SerializeField] private Transform pointBPosition;
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool isAtPointA = true;

        private Transform _targetPosition;
        private bool _moving;
        
        private void Start()
        {
            _targetPosition = isAtPointA ? pointBPosition : pointAPosition;
        }

        private void FixedUpdate()
        {
            if (!_moving)
            {
                return;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, _targetPosition.position) < 0.02f)
            {
                _moving = false;
                transform.position = _targetPosition.position;
                
                isAtPointA = !isAtPointA;
                _targetPosition = isAtPointA ? pointBPosition : pointAPosition;
            }
        }
        
        public void StartElevator()
        {
            if (_moving)
            {
                return;
            }

            _moving = true;
        }

        public bool IsMoving()
        {
            return _moving;
        }
    }
    
}