using Boundary.Player;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Camera
{
    /// <summary>
    /// Moves this layer relative to the main camera to fake depth (parallax scrolling).
    /// Backgrounds live once in the persistent GamePlay scene and are shown/hidden per room by
    /// BackgroundManager rather than being loaded/unloaded with room scenes, so this layer can
    /// sit disabled for an arbitrarily long time while the camera moves anywhere else.
    ///
    /// Position is always computed from a fixed anchor point:
    ///     layer = anchor + (camera - anchor) * factor
    /// so when the camera is centered on the anchor the layer is centered on the camera,
    /// whatever the factor. The anchor is the player's position, reset on every enable (first
    /// show, BackgroundManager switching background) and on every EPlayerTeleported.
    /// The camera may still be far away at that moment (e.g. on load a different vcam activates
    /// a few frames later and blends across the whole map), but the formula doesn't depend on
    /// when the camera arrives: once it settles on the player, the layer is centered on it.
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = moves fully with the level (closest layer, no parallax). " +
                 "1 = moves fully with the camera (farthest layer, looks static on screen, e.g. sky).")]
        [Range(0f, 1f)] [SerializeField] private float parallaxFactorX = 0.5f;
        [Range(0f, 1f)] [SerializeField] private float parallaxFactorY = 0f;

        private IEventBus _eventBus;
        private UnityEngine.Camera _camera;
        private Transform _player;
        private Vector2 _anchor;

        private void OnEnable()
        {
            _camera = UnityEngine.Camera.main;
            _eventBus = GameContext.Instance.EventBus;
            _eventBus.Subscribe<EPlayerTeleported>(OnPlayerTeleported);

            // While this layer was disabled the camera could have moved arbitrarily far away,
            // so never reuse the old anchor.
            if (_player == null)
            {
                var player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
                _player = player != null ? player.transform : null;
            }
            _anchor = _player != null ? _player.position : transform.position;
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<EPlayerTeleported>(OnPlayerTeleported);
        }

        private void OnPlayerTeleported(EPlayerTeleported e)
        {
            _anchor = e.Position;
        }

        // LateUpdate so Cinemachine has already moved the camera for this frame.
        private void LateUpdate()
        {
            if (_camera == null)
            {
                return;
            }

            var offset = (Vector2)_camera.transform.position - _anchor;
            transform.position = new Vector3(
                _anchor.x + offset.x * parallaxFactorX,
                _anchor.y + offset.y * parallaxFactorY,
                transform.position.z);
        }
    }
}
