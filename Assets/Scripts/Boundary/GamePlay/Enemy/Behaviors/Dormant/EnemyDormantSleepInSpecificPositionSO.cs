using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Dormant
{
    [CreateAssetMenu(fileName = "Enemy Dormant Sleep In Specific Position", menuName = "Enemies/Behaviors/Dormant/Enemy Dormant Sleep In Specific Position")]
    public class EnemyDormantSleepInSpecificPositionSO: EnemyDormantSOBase
    {
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private BaseEnemy.WalkDirectionEnum sleepingSide = BaseEnemy.WalkDirectionEnum.Right;
        [SerializeField] private string movementAnimationName = "walk";
        [SerializeField] private string sleepAnimationName = "sleep";
        
        private SearchState _searchState = SearchState.MovingToPosition;
        
        public override void DoEnterLogic()
        {
            // Call base logic
            base.DoEnterLogic();
            _searchState = SearchState.MovingToPosition;
        }

        public override void DoExitLogic()
        {
            
            base.DoExitLogic();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
            
            ChangeSearchStateIfNeeded();
        }
        
        private void ChangeSearchStateIfNeeded()
        {
            var isOnSleepPosition = Vector2.Distance(Enemy.transform.position, Enemy.sleepPosition.transform.position) < 2f;
            
            if (isOnSleepPosition && _searchState != SearchState.Sleeping)
            {
                _searchState = SearchState.Sleeping;
                
                if (Enemy.Animator != null)
                {
                    Enemy.Animator.Play(sleepAnimationName);
                }
            }
            else if (!isOnSleepPosition && _searchState != SearchState.MovingToPosition)
            {
                _searchState = SearchState.MovingToPosition;
                
                if (Enemy.Animator != null)
                {
                    Enemy.Animator.Play(movementAnimationName);
                }
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
            
            switch (_searchState)
            {
                case SearchState.MovingToPosition:
                {
                    var direction = (Enemy.sleepPosition.transform.position - Enemy.transform.position).normalized;
                    direction = new Vector2(direction.x, 0);
                    
                    Enemy.TurnToDirectionIfNeeded(direction.x);
                    
                    Enemy.Rb.linearVelocity = direction * movementSpeed;
                    break;
                }
                case SearchState.Sleeping:
                    Enemy.Rb.linearVelocity = Vector2.zero;
                    
                    if (sleepingSide != Enemy.WalkDirection)
                    {
                        Enemy.Turn();
                    }
                    break;
                default:
                    Debug.LogError("[EnemyDormantSleepInSpecificPositionSO] Invalid search state");
                    break;
            }
        }
    }
    
    internal enum SearchState
    {
        MovingToPosition,
        Sleeping
    }
}