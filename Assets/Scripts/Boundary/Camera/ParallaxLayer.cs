using Boundary.Player;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Camera
{
    /// <summary>
    /// Moves this layer relative to the main camera to fake depth (parallax scrolling).
    /// Backgrounds live once in the persistent GamePlay scene and are shown/hidden per room by
    /// BackgroundManager rather than being loaded/unloaded with room scenes, so this layer can
    /// sit disabled for an arbitrarily long time while the camera moves anywhere else. Every
    /// enable - the very first one, a teleport re-enable, or BackgroundManager re-showing this
    /// background - is treated the same way as EPlayerTeleported: snap straight onto the
    /// camera's current position next LateUpdate, then resume normal factor-based tracking.
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
            _eventBus = GameContext.Instance.EventBus;
            _eventBus.Subscribe<EPlayerTeleported>(OnPlayerTeleported);

            // Don't trust a baseline computed from transform.position here: while this layer was
            // disabled the camera could have moved arbitrarily far away (room streaming, a load,
            // BackgroundManager hiding a different background). Reuse the exact same "snap once,
            // then resume tracking" path already used for EPlayerTeleported instead of a second,
            // parallel positioning mechanism.
            _pendingSnapToCamera = true;
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
