using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Projectile
{
    /// <summary>
    /// Attached at runtime to a Projectile that uses TrackingShotPatternSO.
    /// Steers the projectile toward the player for <see cref="TrackingDuration"/> seconds,
    /// then disables itself and lets the projectile continue in a straight line.
    /// </summary>
    [RequireComponent(typeof(Projectile))]
    public class TrackingProjectileController : MonoBehaviour
    {
        private float _trackingDuration;
        private float _trackingStrength;   // degrees per second

        private float       _elapsed;
        private bool        _tracking;
        private Rigidbody2D _rb;
        private GameObject  _player;

        // ─────────────────────────────────────────────────────────────────────

        public void Init(float duration, float strength)
        {
            _trackingDuration = duration;
            _trackingStrength = strength;
            _elapsed          = 0f;
            _tracking         = true;
            _rb               = GetComponent<Rigidbody2D>();
            _player           = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
        }

        private void Update()
        {
            if (!_tracking) return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _trackingDuration)
            {
                _tracking = false;
                return;
            }

            if (_player == null) return;
            
            var toPlayer = ((Vector2)(_player.transform.position - transform.position)).normalized;
            var currentDir = _rb.linearVelocity.normalized;
            var speed = _rb.linearVelocity.magnitude;

            var maxRotation = _trackingStrength * Time.deltaTime;
            var newDir = Vector2.MoveTowards(currentDir, toPlayer, maxRotation * Mathf.Deg2Rad);

            _rb.linearVelocity = newDir.normalized * speed;
        }

        private void OnDisable()
        {
            _tracking = false;
        }
    }
}

