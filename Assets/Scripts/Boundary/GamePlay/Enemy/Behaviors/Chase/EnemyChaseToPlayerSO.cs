using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Chase {

    [CreateAssetMenu(fileName = "Enemy Chase Direct To Player", menuName = "Enemies/Behaviors/Chase/Enemy Chase Direct To Player")]
    public class EnemyChaseToPlayerSO : EnemyChaseSOBase
    {
        [SerializeField] private float chaseSpeed = 3f;
        [SerializeField] private float chaseContinuationTime;
        
        private float _chaseContinuationTimer;

        private ChaseStatus _chaseStatus = ChaseStatus.ReadyToChase;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();

            _chaseStatus = ChaseStatus.Chasing;

            if (Enemy.Animator)
            {
                Enemy.Animator.Play("walk");
            }
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();

            ResetValues();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();

            if (_chaseStatus == ChaseStatus.LostChase)
            {
                if (chaseContinuationTime <= 0f || _chaseContinuationTimer <= 0f)
                {
                    Enemy.StateMachine.ChangeState(Enemy.IdleState);
                    return;
                }
                
                _chaseContinuationTimer -= Time.deltaTime;
            }
            
            if (!Enemy.IsAggroed)
            {
                _chaseStatus = ChaseStatus.LostChase;
            }
            else
            {
                _chaseStatus = ChaseStatus.Chasing;
                _chaseContinuationTimer = chaseContinuationTime;
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
            
            Enemy.Rb.linearVelocity = new Vector2(chaseSpeed * Enemy.walkDirectionVector.x, Enemy.Rb.linearVelocity.y);
        }

        private void ResetValues()
        {
            _chaseStatus = ChaseStatus.ReadyToChase;
            _chaseContinuationTimer = chaseContinuationTime;
        }

        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();
        }
    }
}