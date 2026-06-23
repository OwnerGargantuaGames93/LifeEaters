using Boundary.Commands;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Interactable.Levers
{
    public class AToBMovingPlatformLever: MonoBehaviour
    {
        private IEventBus _eventBus;
        private bool _playerCanPullLever;
        private LeverState _leverState = LeverState.Off;

        [SerializeField] private GameObject leverOnGameObject;
        [SerializeField] private GameObject leverOffGameObject;
        [SerializeField] private AToBMovingPlatform aToBPlatformToMove;
        
        private bool _isAvailable = true;

        private void Start()
        {
            UpdateLeverVisuals();
        }

        private void UpdateLeverVisuals()
        {
            if (leverOnGameObject == null || leverOffGameObject == null)
            {
                Debug.LogError("Lever GameObjects not assigned");
                return;
            }

            leverOnGameObject.SetActive(_leverState == LeverState.On);
            leverOffGameObject.SetActive(_leverState == LeverState.Off);
        }
        
        private void Update()
        {
            // If the elevator is moving, set availability to false to prevent lever from being pulled again
            _isAvailable = !aToBPlatformToMove.IsMoving();

            if (_isAvailable && _playerCanPullLever && UserInput.instance.GeneralInteractWasPressedThisFrame())
            {
                PullLever();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerCanPullLever = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerCanPullLever = false;
            }
        }
        
        private void PullLever()
        {
            Debug.Log("Pull Lever");
            _leverState = _leverState == LeverState.Off ? LeverState.On : LeverState.Off;
            UpdateLeverVisuals();
            aToBPlatformToMove.StartElevator();
        }
    }

    public enum LeverState
    {
        On,
        Off
    }
}