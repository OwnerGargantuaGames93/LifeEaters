using UnityEngine;
using UnityEngine.Serialization;

namespace Boundary.Utils
{
    public class TouchingDirections : MonoBehaviour
    {
        public ContactFilter2D groundCastFilter;

        public ContactFilter2D wallCastFilter;

        public ContactFilter2D ceilingCastFilter;

        public float groundDistance = 0.05f;
        public float wallDistance = 0.2f;
        public float ceilingDistance = 0.1f;

        private Collider2D _touchingCollider;

        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[8];
        private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[8];
        private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[8];

        [SerializeField] private bool isGrounded = true;
        [SerializeField] private bool isOnWall;
        [SerializeField] private bool isCeiling;

        public bool IsGrounded { get => isGrounded; private set => isGrounded = value; }

        public bool IsOnWall { get => isOnWall; private set => isOnWall = value; }

        public bool IsCeiling { get => isCeiling; private set => isCeiling = value; }

        private Vector2 WallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        private void Awake()
        {
            _touchingCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            DrawGroundCheck();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            IsGrounded = _touchingCollider.Cast(Vector2.down, groundCastFilter, _groundHits, groundDistance) > 0;
            IsOnWall = _touchingCollider.Cast(WallCheckDirection, wallCastFilter, _wallHits, wallDistance) > 0;
            IsCeiling = _touchingCollider.Cast(Vector2.up, ceilingCastFilter, _ceilingHits, ceilingDistance) > 0;
        }

        #region Debug

        private void DrawGroundCheck()
        {
            Color rayColor;

            if (isGrounded)
            {
                rayColor = Color.green;
            } else
            {
                rayColor = Color.red;
            }

            Debug.DrawRay(_touchingCollider.bounds.center + new Vector3(_touchingCollider.bounds.extents.x, 0), Vector2.down * (_touchingCollider.bounds.extents.y + groundDistance), rayColor);
            Debug.DrawRay(_touchingCollider.bounds.center - new Vector3(_touchingCollider.bounds.extents.x, 0), Vector2.down * (_touchingCollider.bounds.extents.y + groundDistance), rayColor);
            Debug.DrawRay(_touchingCollider.bounds.center - new Vector3(_touchingCollider.bounds.extents.x, _touchingCollider.bounds.extents.y + groundDistance), Vector2.right * (_touchingCollider.bounds.extents.x * 2), rayColor);
        }

        #endregion
    }
}
