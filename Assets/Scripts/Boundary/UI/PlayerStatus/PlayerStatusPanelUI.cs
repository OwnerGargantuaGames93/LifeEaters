using System;
using System.Collections.Generic;
using Boundary.UI.GameMenu;
using Control.Pit;
using Control.Player;
using Data.Database;
using Data.Entities.Player;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Boundary.UI.PlayerStatus
{
    public class PlayerStatusPanelUI: MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject contentPanel;

        [SerializeField] private GameObject hpContainer;
        [SerializeField] private GameObject energyContainer;
        [SerializeField] private GameObject lifeContainer;
        
        private readonly List<GameObject> _hpContainerChild = new();
        private readonly List<GameObject> _energyContainerChild = new();
        private readonly List<GameObject> _lifeContainerChild = new();
        
        [SerializeField] private GameObject redHeart;
        [SerializeField] private GameObject emptyRedHeart;
        [SerializeField] private GameObject blackHeart;
        [SerializeField] private GameObject emptyBlackHeart;
        [SerializeField] private GameObject life;
        [SerializeField] private GameObject emptyLife;
        
        [SerializeField] private Image pitObjectImage;
        [SerializeField] private TextMeshProUGUI pitObjectName;
        [SerializeField] private GameObject pitObjectContainer;
        
        [SerializeField] private TextMeshProUGUI coinsCounter;
        [SerializeField] private TextMeshProUGUI pointsCounter;

        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        private void OnEnable()
        {
            contentPanel.SetActive(true);
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            ResetPanel();
            contentPanel.SetActive(false);

            UnsubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EPlayerStatsUpdated>(OnPlayerStatsUpdated);
            _eventBus.Subscribe<EDesiredPitObjectChanged>(OnDesiredPitObjectChanged);
            _eventBus.Subscribe<EOpenGameMenu>(OnGameMenuOpened);
            _eventBus.Subscribe<ECloseGameMenu>(OnGameMenuClosed);
        }
        
        private void UnsubscribeToEvents()
        {
            _eventBus.Unsubscribe<EPlayerStatsUpdated>(OnPlayerStatsUpdated);
            _eventBus.Unsubscribe<EDesiredPitObjectChanged>(OnDesiredPitObjectChanged);
            _eventBus.Unsubscribe<EOpenGameMenu>(OnGameMenuOpened);
            _eventBus.Unsubscribe<ECloseGameMenu>(OnGameMenuClosed);
        }
        
        private void OnPlayerStatsUpdated(EPlayerStatsUpdated e)
        {
            var status = e.Status;
            var attributes = e.Attributes;
      
            RefreshPlayerStatusStatus(status, attributes);
        }
        
        private void OnGameMenuOpened(EOpenGameMenu e)
        {
            contentPanel.SetActive(false);
        }
        
        private void OnGameMenuClosed(ECloseGameMenu e)
        {
            contentPanel.SetActive(true);
        }
        
        private void RefreshPlayerStatusStatus(Data.Entities.Player.PlayerStatus status, PlayerAttributes attributes)
        {
            RefreshHealth(status, attributes);
            RefreshEnergy(status, attributes);
            RefreshLifes(status, attributes);
            RefreshCoins(status);
            RefreshPoints(status);
        }
        
        private void RefreshHealth(Data.Entities.Player.PlayerStatus status, PlayerAttributes attributes)
        {
            var currentHp = status.currentHealth;
            var maxHp = attributes.MaxHealth;
            
            var emptyHeartCount = maxHp - currentHp;

            // Clear existing hearts
            foreach (var heart in _hpContainerChild)
            {
                Destroy(heart);
            }
            _hpContainerChild.Clear();
            
            // Add full hearts
            for (var i = 0; i < currentHp; i++)
            {
                var heart = Instantiate(redHeart, hpContainer.transform);
                _hpContainerChild.Add(heart);
            }
            
            // Add empty hearts
            for (var j = 0; j < emptyHeartCount; j++)
            {
                var emptyHeart = Instantiate(emptyRedHeart, hpContainer.transform);
                _hpContainerChild.Add(emptyHeart);
            }
        }

        private void RefreshEnergy(Data.Entities.Player.PlayerStatus status, PlayerAttributes attributes)
        {
            var currentEnergy = status.currentEnergy;
            var maxEnergy = attributes.MaxEnergy;

            var emptyHeartCount = maxEnergy - currentEnergy;

            // Clear existing hearts
            foreach (var heart in _energyContainerChild)
            {
                Destroy(heart);
            }

            _energyContainerChild.Clear();

            // Add full hearts
            for (var i = 0; i < currentEnergy; i++)
            {
                var heart = Instantiate(blackHeart, energyContainer.transform);
                _energyContainerChild.Add(heart);
            }

            // Add empty hearts
            for (var i = 0; i < emptyHeartCount; i++)
            {
                var heart = Instantiate(emptyBlackHeart, energyContainer.transform);
                _energyContainerChild.Add(heart);
            }
        }

        private void RefreshLifes(Data.Entities.Player.PlayerStatus status, PlayerAttributes attributes)
        {
            var currentLifes = status.currentLifes;
            var maxLifes = attributes.MaxLifes;

            var emptyLifeCount = maxLifes - currentLifes;
            var fullLifeCount = currentLifes;

            // Clear existing lifes
            foreach (var lifeObj in _lifeContainerChild)
            {
                Destroy(lifeObj);
            }

            _lifeContainerChild.Clear();

            // Add full lifes
            for (var i = 0; i < fullLifeCount; i++)
            {
                var lifeObj = Instantiate(life, lifeContainer.transform);
                _lifeContainerChild.Add(lifeObj);
            }

            // Add empty lifes
            for (var i = 0; i < emptyLifeCount; i++)
            {
                var emptyLifeObj = Instantiate(emptyLife, lifeContainer.transform);
                _lifeContainerChild.Add(emptyLifeObj);
            }
        }
        
        private void RefreshCoins(Data.Entities.Player.PlayerStatus status)
        {
            var currentCoins = status.coins;
            coinsCounter.text = currentCoins + "x";
        }
        
        private void RefreshPoints(Data.Entities.Player.PlayerStatus status)
        {
            var currentPoints = status.points;
            pointsCounter.text = currentPoints + "x";
        }

        private void OnDesiredPitObjectChanged(EDesiredPitObjectChanged e)
        {
            var desiredPitObject = e.DesiredObjectId;
            if (desiredPitObject.HasValue)
            {
                pitObjectContainer.SetActive(true);
                var item = DataSource.Instance.GetPitObject(desiredPitObject.Value);
                
                pitObjectImage.sprite = item.Icon;
                pitObjectName.text = item.Name;
            }
            else
            {
                pitObjectContainer.SetActive(false);
                pitObjectImage.sprite = null;
                pitObjectName.text = "";
            }
        }

        private void ResetPanel()
        {
            // Clear health hearts
            foreach (var heart in _hpContainerChild)
            {
                Destroy(heart);
            }

            _hpContainerChild.Clear();

            // Clear energy hearts
            foreach (var heart in _energyContainerChild)
            {
                Destroy(heart);
            }

            _energyContainerChild.Clear();

            // Clear lifes
            foreach (var lifeObj in _lifeContainerChild)
            {
                Destroy(lifeObj);
            }

            _lifeContainerChild.Clear();

            // Reset coins and points
            coinsCounter.text = "0x";
            pointsCounter.text = "0x";

            // Reset pit object display
            pitObjectContainer.SetActive(false);
            pitObjectImage.sprite = null;
            pitObjectName.text = "";
        }
    }
}