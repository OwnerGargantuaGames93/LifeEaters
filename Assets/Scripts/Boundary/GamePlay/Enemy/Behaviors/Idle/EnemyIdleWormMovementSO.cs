using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Worm Movement", menuName = "Enemies/Behaviors/Idle/Enemy Idle Worm Movement")]
    public class EnemyIdleWormMovementSO : EnemyIdleSOBase
    {
        private enum WormState
        {
            Advancing,
            Pausing
        }

        private const string AdvanceAnimationName = "advance";
        private const string PauseAnimationName = "idle";

        [SerializeField] private float stepSpeed = 1f;
        [SerializeField] private float stepDistance = 0.5f;
        [SerializeField] private float pauseDuration = 0.5f;
        [SerializeField] private float waitAfterCliffDetection;

        private WormState _wormState;
        private float _stepStartX;
        private float _pauseTimer;
        private bool _isStarted = false;
        private float _currentWaitAfterCliffDetection;

        private bool HasCliffDetection => Enemy.cliffDetection != null;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            
            Enemy.Animator.Play("falling");
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            Enemy.Rb.linearVelocity = Vector2.zero;
            _currentWaitAfterCliffDetection = 0f;
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();

            // TODO: In the first frame seems to be true even if is not. Check this.
            if (!_isStarted && Enemy.TouchingDirections.IsGrounded)
            {
                StartAdvancing();
                _isStarted = true;
            }
            
            switch (_wormState)
            {
                case WormState.Advancing:
                    if (Mathf.Abs(Transform.position.x - _stepStartX) >= stepDistance)
                    {
                        StartPausing();
                    }
                    break;
                case WormState.Pausing:
                    _pauseTimer -= Time.deltaTime;
                    if (_pauseTimer <= 0f)
                    {
                        StartAdvancing();
                    }
                    break;
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();

            if (Enemy.isKnockedBack || !Enemy.TouchingDirections.IsGrounded)
            {
                return;
            }

            Enemy.Rb.linearVelocity = _wormState == WormState.Advancing
                ? new Vector2(stepSpeed * Enemy.walkDirectionVector.x, Enemy.Rb.linearVelocity.y)
                : new Vector2(0f, Enemy.Rb.linearVelocity.y);

            HandleWallAndCliffDetection();
        }

        private void StartAdvancing()
        {
            _wormState = WormState.Advancing;
            _stepStartX = Transform.position.x;
            Enemy.Animator.Play(AdvanceAnimationName);
        }

        private void StartPausing()
        {
            _wormState = WormState.Pausing;
            _pauseTimer = pauseDuration;
            Enemy.Rb.linearVelocity = Vector2.zero;
            Enemy.Animator.Play(PauseAnimationName);
        }

        private void PerformTurn()
        {
            Enemy.Turn();
            _stepStartX = Transform.position.x;
        }

        private void HandleWallAndCliffDetection()
        {
            if (Enemy.TouchingDirections.IsGrounded && Enemy.TouchingDirections.IsOnWall)
            {
                PerformTurn();
                return;
            }

            HandleCliffDetection();
        }

        private void HandleCliffDetection()
        {
            if (!HasCliffDetection)
            {
                return;
            }

            if (Enemy.turnWasPerformed)
            {
                if (Enemy.doubleTurnTimeBuffer < Enemy.doubleTurnPreventionTime)
                {
                    Enemy.doubleTurnTimeBuffer += Time.deltaTime;
                }
                else
                {
                    Enemy.turnWasPerformed = false;
                    Enemy.doubleTurnTimeBuffer = 0f;
                }

                return;
            }

            if (!Enemy.TouchingDirections.IsGrounded)
            {
                return;
            }

            var cliffDetected = Enemy.cliffDetection.DetectedColliders.Count == 0 && Enemy.cliffDetection.readyForFixedUpdate;
            if (!cliffDetected)
            {
                return;
            }

            if (waitAfterCliffDetection > 0f)
            {
                _currentWaitAfterCliffDetection += Time.deltaTime;
                if (_currentWaitAfterCliffDetection < waitAfterCliffDetection)
                {
                    return;
                }

                PerformTurn();
                _currentWaitAfterCliffDetection = 0f;
            }
            else
            {
                PerformTurn();
            }
        }
    }
}
