using Infra.EventBus;
using Infra.SceneHandler;
using UnityEngine;
using Utils;

namespace Boundary.Interactable
{
    public class ChangeSceneTrigger: MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [SerializeField] private string targetScene;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                _eventBus.Publish(new EPlayerEnterScene(targetScene));
            }
        }
    }
}