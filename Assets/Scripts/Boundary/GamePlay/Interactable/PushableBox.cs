using Control.Player;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Interactable
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PushableBox: MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 3f;
        [SerializeField] private float damping = 5f;
        [SerializeField] private int minimumStrength = 1;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            // stop movement if player strength is insufficient
            if (GameContext.Instance.Player.Characteristics.Strength < minimumStrength)
            {
                _rb.linearVelocity = Vector2.zero;
            }

            // limit max speed
            if (_rb.linearVelocity.magnitude > maxSpeed)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
            }

            // apply damping
            if (_rb.linearVelocity.magnitude > 0.01f)
            {
                _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, damping * Time.fixedDeltaTime);
            }
        }
    }
}