using Boundary.Utils;
using Control.GameData;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Collectibles
{
    public class Coin : IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        private IGameDataModel _gameData;

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
        }
    
        private void Start()
        {
            if (_gameData.IsObjectCollected(Id))
            {  
                // If the coin has already been collected, disable it
                gameObject.SetActive(false);
            }
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.PlayerTag))
            {
                return;
            }
            
            _eventBus.Publish(new ECoinCollected(Id));
            
            Destroy(gameObject);
        }
        
    }
    
    #region Events
    
    public struct ECoinCollected
    {
        public readonly string CoinId;

        public ECoinCollected(string coinId)
        {
            CoinId = coinId;
        }
    }
    
    #endregion
}
