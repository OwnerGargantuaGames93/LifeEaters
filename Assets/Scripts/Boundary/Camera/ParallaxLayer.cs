using Boundary.Player;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Camera
{
    /// <summary>
    /// Moves this layer relative to the main camera to fake depth (parallax scrolling).
    /// Lives as a single persistent scene-level object rather than being toggled per room,
    /// so its baseline is only ever established once. A hard teleport (statue, etc.) can
    /// land the camera far from that baseline - too far for the parallax factor to track
    /// without leaving a gap - so on EPlayerTeleported this snaps straight onto the camera's
    /// current position instead, then resumes normal factor-based tracking from there.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = moves fully with the level (closest layer, no parallax). " +
                 "1 = moves fully with the camera (farthest layer, looks static on screen, e.g. sky).")]
        [Range(0f, 1f)] [SerializeField] private float parallaxFactorX = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float parallaxFactorY = 0f;

        private IEventBus _eventBus;
        private UnityEngine.Camera _camera;
        private Vector3 _startPosition;
        private Vector3 _startCameraPosition;
        private bool _pendingSnapToCamera;

        private void OnEnable()
        {
            _camera = UnityEngine.Camera.main;
            _startPosition = transform.position;

            if (_camera != null)
            {
                _startCameraPosition = _camera.transform.position;
            }

            _eventBus = GameContext.Instance.EventBus;
            _eventBus.Subscribe<EPlayerTeleported>(OnPlayerTeleported);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<EPlayerTeleported>(OnPlayerTeleported);
        }

        private void OnPlayerTeleported(EPlayerTeleported e)
        {
            _pendingSnapToCamera = true;
        }

        // LateUpdate so Cinemachine (including the teleport warp) has already moved the
        // camera for this frame.
        private void LateUpdate()
        {
            if (_camera == null)
            {
                return;
            }

            if (_pendingSnapToCamera)
            {
                transform.position = new Vector3(
                    _camera.transform.position.x,
                    _camera.transform.position.y,
                    transform.position.z);
                _startPosition = transform.position;
                _startCameraPosition = _camera.transform.position;
                _pendingSnapToCamera = false;
                return;
            }

            var cameraDelta = _camera.transform.position - _startCameraPosition;
            transform.position = _startPosition + new Vector3(
                cameraDelta.x * parallaxFactorX,
                cameraDelta.y * parallaxFactorY,
                0f);
        }
    }
}
