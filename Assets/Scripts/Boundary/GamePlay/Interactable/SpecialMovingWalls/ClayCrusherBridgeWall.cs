using Boundary.Interactable.Levers;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Interactable
{
    public class ClayCrusherBridgeWall: MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [SerializeField] private Transform upperTargetPoint;
        [SerializeField] private Transform lowerTargetPoint;
        [SerializeField] private float speed = 2f;
        [SerializeField] private WallState currentState = WallState.Up;
        
        private bool _isMoving;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
        }

        public void Move()
        {
            _isMoving = true;
        }

        public bool IsMoving()
        {
            return _isMoving;
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            var targetPoint = currentState == WallState.Up ? lowerTargetPoint : upperTargetPoint;
            
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPoint.position,
                speed * Time.fixedDeltaTime
            );

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.01f)
            {
                transform.position = targetPoint.position;
                currentState = currentState == WallState.Up ? WallState.Down : WallState.Up;
                _isMoving = false;
                _eventBus.Publish(new EHomeRouteBridgeMoved());
            }
        }
        
    }
    
    internal enum WallState
    {
        Up,
        Down
    }
    
    #region Events
    public struct EHomeRouteBridgeMoved {}
    
    #endregion
    
}