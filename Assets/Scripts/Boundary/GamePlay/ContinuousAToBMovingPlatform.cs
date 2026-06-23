using UnityEngine;

namespace Boundary.GamePlay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ContinuousAToBMovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector2 moveOffset;
        [SerializeField] private float speed = 2f;

        private Rigidbody2D _rb;
        private Vector2 _pointA;
        private Vector2 _pointB;
        private Vector2 _target;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void Start()
        {
            _pointA = _rb.position;
            _pointB = _pointA + moveOffset;
            _target = _pointB;
        }

        private void FixedUpdate()
        {
            var newPos = Vector2.MoveTowards(_rb.position, _target, speed * Time.fixedDeltaTime);
            _rb.MovePosition(newPos);

            if (Vector2.Distance(newPos, _target) < 0.01f)
                _target = _target == _pointB ? _pointA : _pointB;
        }

        private void OnDrawGizmosSelected()
        {
            var a = Application.isPlaying ? (Vector3)_pointA : transform.position;
            var b = a + (Vector3)moveOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(a, 0.15f);
            Gizmos.DrawWireSphere(b, 0.15f);
            Gizmos.DrawLine(a, b);
        }
    }
}
