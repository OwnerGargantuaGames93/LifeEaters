using Boundary.Player;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Interactable.SpecialTriggers
{
    public class TeleportToRswo : MonoBehaviour
    {
        
        private IEventBus _eventBus;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                Teleport();
            }
        }
        
        private void Teleport()
        {
            _eventBus.Publish(new ETeleportPlayerToPosition(Constants.RswoRespawnPoint));
        }
        
    }   
}
