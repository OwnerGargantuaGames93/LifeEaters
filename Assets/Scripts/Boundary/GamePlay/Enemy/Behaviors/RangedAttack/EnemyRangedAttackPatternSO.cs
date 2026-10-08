using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Enemy.Behaviors.Chase;
using Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns;
using Boundary.GamePlay.Projectile; // EnemyRangedAttackSOBase lives here
using Data.Entities.Enemy;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack
{
    /// <summary>
    /// Ranged-attack behaviour SO that uses the new projectile-pool system.
    /// Assign a RangedAttackPatternSOBase subclass (SingleShot, Spread, Burst, etc.)
    /// to drive what gets fired and how.
    ///
    /// If phase2Pattern is assigned, it will be used when the enemy is in Phase2 or higher.
    ///
    /// Workflow:
    ///   EnterState  → optional attack animation (+ optional delay), then call pattern.Execute()
    ///   ExitState   → transition to CooldownState
    ///   OnReceiveDamage → interrupt and go to IdleState
    /// </summary>
    [CreateAssetMenu(fileName = "RangedAttackPattern",
        menuName = "Enemies/Behaviors/Ranged Attack/Pattern Based Ranged Attack")]
    public class EnemyRangedAttackPatternSO : EnemyRangedAttackSOBase
    {
        [Header("Pattern — Phase 1")]
        [SerializeField] private RangedAttackPatternSOBase pattern;

        [Header("Pattern — Phase 2+ (optional, leave empty to reuse Phase 1 pattern)")]
        [SerializeField] private RangedAttackPatternSOBase phase2Pattern;

        [Header("Optional — if true, waits for the attack animation to finish before firing")]
        [SerializeField] private bool waitForAnimation = false;
        
        [SerializeField] private string attackAnimationName = "attack";

        [Tooltip("Seconds to wait after the attack animation ends before releasing the projectile")]
        [SerializeField, Min(0f)] private float delayAfterAnimation = 0f;
        
        [Header("Optional — if true, will be a cooldown state after firing, otherwise will go to IdleState")]
        [SerializeField] private bool goToCooldownAfterFiring = true;
        
        [Header("Optional - always looking player while shooting")]
        [SerializeField] private bool lookAtPlayerWhileShooting = false;

        private bool _hasFired;
        private bool _waitingForAnim;
        private bool _animFinished;
        private float _delayTimer;

        /// <summary>Returns the correct pattern based on the enemy's current phase.</summary>
        private RangedAttackPatternSOBase ActivePattern
        {
            get
            {
                if (phase2Pattern != null && Enemy.CurrentPhase() >= EnemyPhase.Phase2)
                    return phase2Pattern;
                return pattern;
            }
        }

        // ─────────────────────────────────────────────────────────────────────

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            _hasFired       = false;
            _waitingForAnim = false;
            _animFinished   = false;
            _delayTimer     = 0f;

            FaceTowardsPlayer();

            if (waitForAnimation && Enemy.Animator != null)
            {
                Enemy.Animator.Play(attackAnimationName);
                _waitingForAnim = true;
            }
            else
            {
                Fire();
            }
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();

            if (!_waitingForAnim || _hasFired) return;

            FaceTowardsPlayer();

            // Wait until the attack animation reaches its end
            if (!_animFinished)
            {
                if (Enemy.Animator == null) return;
                var stateInfo = Enemy.Animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(attackAnimationName) || stateInfo.normalizedTime < 1f) return;
                _animFinished = true;
            }

            // Then wait the configured delay before releasing the projectile
            _delayTimer += Time.deltaTime;
            if (_delayTimer >= delayAfterAnimation)
            {
                Fire();
            }
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            ResetValues();
        }

        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();
            Enemy.StateMachine.ChangeState(Enemy.IdleState);
        }

        protected override void ResetValues()
        {
            base.ResetValues();
            _hasFired       = false;
            _waitingForAnim = false;
            _animFinished   = false;
            _delayTimer     = 0f;
        }

        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// If lookAtPlayerWhileShooting is enabled, flips only the enemy's sprite to face the player
        /// (via Enemy.SetSpriteFacing). Does NOT touch transform.localScale/WalkDirection, so child
        /// colliders (aggro/attack detection zones) never move — important for stationary shooters.
        /// </summary>
        private void FaceTowardsPlayer()
        {
            if (!lookAtPlayerWhileShooting || Enemy.Player == null) return;

            var directionToPlayerX = Enemy.Player.transform.position.x - Enemy.transform.position.x;
            if (directionToPlayerX > 0)
            {
                Enemy.SetSpriteFacing(BaseEnemy.WalkDirectionEnum.Right);
            } else if (directionToPlayerX < 0)
            {
                Enemy.SetSpriteFacing(BaseEnemy.WalkDirectionEnum.Left);
            }
        }

        private void Fire()
        {
            if (_hasFired) return;
            _hasFired       = true;
            _waitingForAnim = false;

            var activePattern = ActivePattern;
            if (activePattern == null)
            {
                Debug.LogWarning($"[EnemyRangedAttackPatternSO] No pattern assigned on {name}.");
                Enemy.StateMachine.ChangeState(Enemy.CooldownState);
                return;
            }

            var pool = ProjectilePool.Instance;
            if (pool == null)
            {
                Debug.LogError("[EnemyRangedAttackPatternSO] ProjectilePool.Instance is null. " +
                               "Make sure ProjectilePool is present in the GamePlay scene.");
                Enemy.StateMachine.ChangeState(Enemy.CooldownState);
                return;
            }

            activePattern.Execute(Enemy, pool);
            
            if (!goToCooldownAfterFiring)
            {
                Enemy.StateMachine.ChangeState(Enemy.IdleState);
                return;
            }

            Enemy.StateMachine.ChangeState(Enemy.CooldownState);
        }
    }
}



