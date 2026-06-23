using Boundary.GamePlay.Enemy.Behaviors.Dormant;
using Boundary.Interactable;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Dormant
{
    [CreateAssetMenu(fileName = "Popjue Bear Want To Sleep", menuName = "Enemies/Behaviors/Dormant/Popjue Bear Want To Sleep")]
    public class PopjueBearDormantStateSO: EnemyDormantSleepInSpecificPositionSO
    {
        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            
            SubscribeToEvents();
        }
        
        public override void DoExitLogic()
        {
            base.DoExitLogic();
            
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            GameContext.Instance.EventBus.Subscribe<EHomeRouteBridgeMoved>(OnHomeRouteBridgeMoved);
        }
        
        private void UnsubscribeFromEvents()
        {
            GameContext.Instance.EventBus.Unsubscribe<EHomeRouteBridgeMoved>(OnHomeRouteBridgeMoved);
        }
        
        private void OnHomeRouteBridgeMoved(EHomeRouteBridgeMoved evt)
        {
            Enemy.StateMachine.ChangeState(Enemy.IdleState);
        }
        
        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();
            
            Enemy.StateMachine.ChangeState(Enemy.IdleState);
        }
    }
}