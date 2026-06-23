using UnityEngine;

namespace Boundary.Interactable
{
    public class SewersPuzzlePlatform : MonoBehaviour
    {
        private const float HeightPerLevel = 2.5f;
        private const int MinLevel = 0;
        private const int MaxLevel = 2;

        private const float speed = 5f;

        private Vector3 _basePosition;
        private int _currentLevel;
        private Vector3 _targetPosition;
        private bool _isMoving;

        private void Start()
        {
            _basePosition = transform.position;
            _targetPosition = _basePosition;
        }

        private void FixedUpdate()
        {
            if (!_isMoving) return;

            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, _targetPosition) < 0.02f)
            {
                transform.position = _targetPosition;
                _isMoving = false;
            }
        }

        public bool TryMove(int delta)
        {
            if (_isMoving) return false;

            var targetLevel = Mathf.Clamp(_currentLevel + delta, MinLevel, MaxLevel);
            if (targetLevel == _currentLevel) return false;

            _currentLevel = targetLevel;
            _targetPosition = _basePosition + Vector3.up * (_currentLevel * HeightPerLevel);
            _isMoving = true;
            return true;
        }

        public bool IsMoving() => _isMoving;
    }
}
