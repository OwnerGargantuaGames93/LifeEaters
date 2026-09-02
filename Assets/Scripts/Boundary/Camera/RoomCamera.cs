using Cinemachine;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Camera
{
    /// <summary>
    /// This script represents a Room, which is a room viewed at a certain moment by the game.
    /// It has a trigger that activates and deactivates the associated virtual camera.
    /// </summary>
    public class RoomCamera : MonoBehaviour
    {
        private IEventBus _eventBus;

        public GameObject virtualCamera;

        private GameObject _player;
        private Collider2D _playerCollider;
        private PolygonCollider2D _roomCollider;

        private bool _isPlayerNearRoom;
        private bool _cameraActive;

        [SerializeField] private string roomName;
        
        [SerializeField] public bool shouldCameraFollowPlayer = true;

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
            _playerCollider = _player.GetComponent<BoxCollider2D>();
            _roomCollider = GetComponent<PolygonCollider2D>();
            _eventBus = GameContext.Instance.EventBus;

            roomName ??= gameObject.name;
        }

        private void Start()
        {
            if (!shouldCameraFollowPlayer)
            {
                return;
            }

            virtualCamera.GetComponent<CinemachineVirtualCamera>()
                .AddCinemachineComponent<CinemachineFramingTransposer>();
            virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = _player.transform;
            virtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = _player.transform;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _isPlayerNearRoom = true;
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _isPlayerNearRoom = false;
            }
        }

        private void Update()
        {
            if (_isPlayerNearRoom && IsPlayerFullyInsideRoom())
            {
                if (_cameraActive)
                {
                    return;
                }

                virtualCamera.SetActive(true);
                _cameraActive = true;
                _eventBus.Publish(new EPlayerEnteredRoom(roomName));
            }
            else
            {
                if (!_cameraActive)
                {
                    return;
                }

                virtualCamera.SetActive(false);
                _cameraActive = false;
                _eventBus.Publish(new EPlayerExitedRoom(roomName));
            }
        }

        private bool IsPlayerFullyInsideRoom()
        {
            var bounds = _playerCollider.bounds;
            var room = _roomCollider;
            if (room is null)
            {
                return false;
            }
            var min = bounds.min;
            var max = bounds.max;
            var corners = new Vector2[] {
                min,
                new Vector2(min.x, max.y),
                new Vector2(max.x, min.y),
                max
            };

            foreach (var corner in corners)
            {
                if (!room.OverlapPoint(corner))
                {
                    return false;   
                }
            }

            return true;
        }
    }
    
    #region Events
    
    public struct EPlayerEnteredRoom
    {
        public string RoomName;
        
        public EPlayerEnteredRoom(string roomName)
        {
            RoomName = roomName;
        }
    }
    
    public struct EPlayerExitedRoom
    {
        public string RoomName;
        
        public EPlayerExitedRoom(string roomName)
        {
            RoomName = roomName;
        }
    }
    
    #endregion
}
