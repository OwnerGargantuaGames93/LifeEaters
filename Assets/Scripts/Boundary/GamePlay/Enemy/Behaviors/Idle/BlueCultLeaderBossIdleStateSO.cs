using System;
using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Enemy.Behaviors.Idle;
using Data.Entities.Enemy;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Blue Cult Leader", menuName = "Enemies/Behaviors/Idle/Idle Blue Cult Leader")]
    public class BlueCultLeaderBossIdleStateSO: EnemyIdleSOBase {
        [SerializeField] private float phase1DecisionTimeMin = 1.5f;
        [SerializeField] private float phase1DecisionTimeMax = 2.2f;
        [SerializeField] private float phase2DecisionTimeMin = 0.7f;
        [SerializeField] private float phase2DecisionTimeMax = 1.2f;

        [Header("Chase Reaction")]
        [Tooltip("Before starting the chase, the boss stays still playing this animation for chaseReactionDuration seconds")]
        [SerializeField] private string chaseReactionAnimationName = "agroed_chase";
        [SerializeField, Min(0f)] private float chaseReactionDuration = 0.5f;

        private float _decisionTime;
        private float _decisionTimer;

        private bool _isReactingBeforeChase;
        private float _chaseReactionTimer;
        
        private int _lastActionConsecutiveCount;
        private bool _lastActionWasChase;
        
        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            _decisionTimer = 0f;
            ResetChaseReaction();
            
            // Choose a random number between phase 1 min and max
            _decisionTime = Enemy.CurrentPhase() switch
            {
                EnemyPhase.Phase1 => UnityEngine.Random.Range(phase1DecisionTimeMin, phase1DecisionTimeMax),
                EnemyPhase.Phase2 => UnityEngine.Random.Range(phase2DecisionTimeMin, phase2DecisionTimeMax),
                _ => throw new ArgumentOutOfRangeException("Invalid enemy phase for Wso Boss Idle State: " +
                                                           Enemy.CurrentPhase())
            };
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            _decisionTime = 1f;
            _decisionTimer = 0f;
            ResetChaseReaction();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();

            // Reazione prima del chase: resta fermo con l'animazione, poi parte il chase
            if (_isReactingBeforeChase)
            {
                _chaseReactionTimer += Time.deltaTime;
                if (_chaseReactionTimer >= chaseReactionDuration)
                {
                    Enemy.StateMachine.ChangeState(Enemy.ChasingState);
                }
                return;
            }

            if (_decisionTimer < _decisionTime)
            {
                _decisionTimer += Time.deltaTime;
                return;
            }

            ChooseAction();
        }

        public override void DoPhysicsLogic()
        {
            // Stay still and turn face to player
            var player = Enemy.Player;
            if (
                player.transform.position.x < Enemy.transform.position.x && Enemy.WalkDirection == BaseEnemy.WalkDirectionEnum.Right ||
                player.transform.position.x > Enemy.transform.position.x && Enemy.WalkDirection == BaseEnemy.WalkDirectionEnum.Left)
            {
                Enemy.Turn();
            }

            base.DoPhysicsLogic();
        }

        private void ChooseAction()
        {
            var randomValue = UnityEngine.Random.value;
            bool wantsChase = randomValue < 0.4f;

            // Se ha già fatto la stessa scelta 2 volte di fila, forza l'alternativa
            if (_lastActionConsecutiveCount >= 2 && wantsChase == _lastActionWasChase)
                wantsChase = !wantsChase;

            // Aggiorna il contatore
            if (wantsChase == _lastActionWasChase)
                _lastActionConsecutiveCount++;
            else
            {
                _lastActionConsecutiveCount = 1;
                _lastActionWasChase = wantsChase;
            }

            if (wantsChase)
                StartChaseReaction();
            else
                Enemy.StateMachine.ChangeState(Enemy.RangedAttackState);
        }

        private void StartChaseReaction()
        {
            _isReactingBeforeChase = true;
            _chaseReactionTimer = 0f;

            if (Enemy.Animator && !string.IsNullOrEmpty(chaseReactionAnimationName))
            {
                Enemy.Animator.Play(chaseReactionAnimationName);
            }
        }

        private void ResetChaseReaction()
        {
            _isReactingBeforeChase = false;
            _chaseReactionTimer = 0f;
        }
    }
}