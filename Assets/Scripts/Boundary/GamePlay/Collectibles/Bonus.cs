using Boundary.Utils;
using Control.GameData;
using Data.Entities.Collectibles;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Collectibles
{
    public class Bonus : IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        private IGameDataModel _gameData;
        
        #region Unity Editor + Data
        [field: SerializeField] private BonusType Type { get; set; }
        [field: SerializeField] private int CustomPointsAmount { get; set; }
        private BonusCollectibleData _data;
        #endregion

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
        }

        private void Start()
        {
            _data = new BonusCollectibleData(Type, CustomPointsAmount, Id);
            
            if (_gameData.IsObjectCollected(Id))
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.PlayerTag))
            {
                return;
            }
            
            _eventBus.Publish(new EBonusCollected(_data));
            
            Destroy(gameObject);
        }
    }

    #region Events

    public struct EBonusCollected
    {
        public readonly BonusCollectibleData BonusData;

        public EBonusCollected(BonusCollectibleData bonusData)
        {
            BonusData = bonusData;
        }
    }

    #endregion
}
