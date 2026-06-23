using Boundary.Enemy.Behaviors.Idle;
using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Left And Right", menuName = "Enemies/Behaviors/Idle/Enemy Idle Left And Right")]
    public class EnemyIdleLeftAndRightSO : EnemyIdleSOBase
    {
        private float _currentWaitAfterCliffDetection;

        private bool HasCliffDetection => Enemy.cliffDetection != null;

        [SerializeField] private float idleMovementSpeed = 1.5f;
        [SerializeField] private float waitAfterCliffDetection;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
        
            // Stop enemy movement
            Enemy.Rb.linearVelocity = Vector2.zero;
        
            // Reset the wait after cliff detection
            _currentWaitAfterCliffDetection = 0f;
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
            
            // Se il nemico non ha un pavimento sotto di sé, non fare nulla
            if (!Enemy.TouchingDirections.IsGrounded)
            {
                return;
            }
        
            // Se non è in fase di knock back e non è in attesa dopo la rilevazione di un burrone
            if (!Enemy.isKnockedBack && _currentWaitAfterCliffDetection == 0f)
            {
                Enemy.Rb.linearVelocity = new Vector2(idleMovementSpeed * Enemy.walkDirectionVector.x, Enemy.Rb.linearVelocity.y);
            }

            // Se è in attesa dopo la rilevazione di un burrone deve stare fermo
            if (_currentWaitAfterCliffDetection > 0f) {
                Enemy.Rb.linearVelocity = new Vector2(0, Enemy.Rb.linearVelocity.y);
            }
            
            if (Enemy.TouchingDirections.IsGrounded && Enemy.TouchingDirections.IsOnWall) {
                Enemy.Turn();
            }

            HandleCliffDetection();
        }

        private void HandleCliffDetection()
        {
            // Se possiede la detection di un burrone
            if (!HasCliffDetection)
            {
                return;
            }

            // Double turn prevention
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
                // Se è a terra
            } else if (Enemy.TouchingDirections.IsGrounded) 
            {
                // Se non ci sono collider rilevati e la detection è pronta per il FixedUpdate
                var cliffDetected = Enemy.cliffDetection.DetectedColliders.Count == 0 && Enemy.cliffDetection.readyForFixedUpdate;

                // Se è stato rilevato un burrone
                if (!cliffDetected)
                {
                    return;
                }

                // Se è stato impostato un tempo di attesa dopo la rilevazione di un burrone (aka attesa attiva)
                if (waitAfterCliffDetection > 0f) {
                    // Incrementa il tempo di attesa
                    _currentWaitAfterCliffDetection += Time.deltaTime;

                    // Se il tempo di attesa è terminato
                    if (!(_currentWaitAfterCliffDetection >= waitAfterCliffDetection))
                    {
                        return;
                    }

                    // Gira la direzione di movimento
                    Enemy.Turn();

                    // Resetta il tempo di attesa
                    _currentWaitAfterCliffDetection = 0f;
                } else {
                    // Gira la direzione di movimento
                    Enemy.Turn();
                }
            }
        }
    }
}
