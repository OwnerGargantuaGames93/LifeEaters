using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Look Left And Right", menuName = "Enemies/Behaviors/Idle/Enemy Idle Look Left And Right")]
    public class EnemyIdleLookLeftAndRightSO : EnemyIdleSOBase
    {
        [SerializeField] private float turnInterval = 2f;

        [Header("Aggro Reaction")]
        [Tooltip("If enabled, when the player enters aggro, the enemy will stay still and play the reaction animation before switching to chase")]
        [SerializeField] private bool reactBeforeChase;
        [SerializeField] private float aggroReactionDuration = 0.5f;
        [SerializeField] private string aggroReactionAnimationName = "agroed";

        private float _turnTimer = 0f;
        private bool _isReactingToAggro;
        private float _aggroReactionTimer;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();

            ResetAggroReaction();

            if (Enemy.Animator)
            {
                Enemy.Animator.Play("idle");
            }
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();

            ResetAggroReaction();
        }

        public override void DoFrameUpdateLogic()
        {
            // Reazione all'aggro in corso: resta fermo finché il timer non scade, poi parte il chase
            if (_isReactingToAggro)
            {
                _aggroReactionTimer += Time.deltaTime;

                if (_aggroReactionTimer >= aggroReactionDuration)
                {
                    Enemy.StateMachine.ChangeState(Enemy.ChasingState);
                }

                return;
            }

            if (reactBeforeChase && !DisableAggroCheck && Enemy.IsAggroed)
            {
                StartAggroReaction();
                return;
            }

            base.DoFrameUpdateLogic();

            _turnTimer += Time.deltaTime;

            if (!(_turnTimer >= turnInterval))
            {
                return;
            }

            Enemy.Turn();
            _turnTimer = 0f;
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();

            if (_isReactingToAggro)
            {
                Enemy.Rb.linearVelocity = new Vector2(0f, Enemy.Rb.linearVelocity.y);
            }
        }

        private void StartAggroReaction()
        {
            _isReactingToAggro = true;
            _aggroReactionTimer = 0f;

            if (Enemy.Animator && !string.IsNullOrEmpty(aggroReactionAnimationName))
            {
                Enemy.Animator.Play(aggroReactionAnimationName);
            }
        }

        private void ResetAggroReaction()
        {
            _isReactingToAggro = false;
            _aggroReactionTimer = 0f;
        }
    }
}
