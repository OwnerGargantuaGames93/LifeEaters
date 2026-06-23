using System.Collections.Generic;
using System.Linq;
using Boundary.Commands;
using Boundary.Utils;
using Control.GameData;
using Control.Inventory;
using Data.Entities.Game;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Boundary.Interactable
{
    public class Door : IdentifiableMonoBehaviour
    {
        private IInventoryModel _inventory;
        private IEventBus _eventBus;
        private IGameDataModel _gameData;
        
        private GameObject _closedDoor;
        private GameObject _openDoor;

        private bool _playerCanOpenDoor;
        private GameObject _player;

        [SerializeField] public KeyId requiredItemId;
        [SerializeField] public bool isLocked;
        [SerializeField] public Tilemap coverTilemap;
        [SerializeField] private bool isOpen = false;
        [SerializeField] public DoorOpenDirection openDirection = DoorOpenDirection.Both;
        [SerializeField] private List<GameplayEventId> requiredGameplayEvents;

        private void Awake()
        {
            CommonAwake();
            
            _inventory = GameContext.Instance.Inventory;
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
            
            _player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            // Pick the child object with name "ClosedDoor" and "OpenDoor"
            _closedDoor = transform.Find("ClosedDoor").gameObject;
            _openDoor = transform.Find("OpenDoor").gameObject;
            
            if (_gameData.IsDoorOpened(Id))
            {  
                isOpen = true;
            }

            OnDoorStatusChange();

            if (requiredGameplayEvents is { Count: > 0 })
            {
                _eventBus.Subscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
                if (!isOpen && AllRequiredEventsOccurred())
                {
                    OpenDoor();
                }
            }
        }

        private void OnDestroy()
        {
            if (requiredGameplayEvents is { Count: > 0 })
            {
                _eventBus.Unsubscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
            }
        }

        private void OnGameplayEventOccurred(EGameplayEventOccurred e)
        {
            if (isOpen || !AllRequiredEventsOccurred())
            {
                return;
            }

            OpenDoor();
        }

        private bool AllRequiredEventsOccurred()
        {
            return requiredGameplayEvents.All(id => _gameData.HasGameplayEvent(id.ToString()));
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_playerCanOpenDoor || isOpen)
            {
                return;
            }
            
            // Check if the player is in the correct position to open the door based on the openDirection setting
            if (!CanOpenFromPlayerPosition())
            {
                return;
            }
            
            if (!isLocked)
            {
                // If the player is pressing the interact button, open the door
                // TODO: Add interact button press in UserInput
                if (UserInput.instance.OpenDoorIsPressed())
                {
                    OpenDoor();
                }
            }
            else
            {
                // var hasRequiredObject = InventoryManager.Instance.HasKeyObject(requiredItemId);
                var hasRequiredObject = _inventory.HasKeyObject(requiredItemId);
                
                if (hasRequiredObject && UserInput.instance.OpenDoorIsPressed())
                {
                    // Open the door
                    OpenDoor();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerCanOpenDoor = true;
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerCanOpenDoor = false;
            }
        }
        
        private bool CanOpenFromPlayerPosition()
        {
            var playerPosition = _player.transform.position;
            
            if (openDirection == DoorOpenDirection.Both)
            {
                return true;
            }
            
            // Calcola se il giocatore è a sinistra o a destra della porta
            var playerIsOnLeft = playerPosition.x < _closedDoor.transform.position.x;
            
            switch (openDirection)
            {
                case DoorOpenDirection.LeftOnly when playerIsOnLeft:
                case DoorOpenDirection.RightOnly when !playerIsOnLeft:
                case DoorOpenDirection.Both:
                    return true;
                default:
                    return false;
            }
        }
    
        private void OnDoorStatusChange()
        {
            if (isOpen)
            {
                _closedDoor.SetActive(false);
                _openDoor.SetActive(true);
                
                if (coverTilemap)
                {
                    coverTilemap.gameObject.SetActive(false);
                }
            }
            else
            {
                _closedDoor.SetActive(true);
                _openDoor.SetActive(false);
                
                if (coverTilemap)
                {
                    coverTilemap.gameObject.SetActive(true);
                }
            }
        }

        private void OpenDoor()
        {
            isOpen = true;
            
            _eventBus.Publish(new EDoorOpened(Id));
        
            OnDoorStatusChange();
        }
    }
    
    public enum DoorOpenDirection
    {
        Both,
        LeftOnly,
        RightOnly
    }
    
    #region Doors
    
    public struct EDoorOpened
    {
        public readonly string DoorId;
        
        public EDoorOpened(string doorId)
        {
            DoorId = doorId;
        }
    }
    
    #endregion
    
}
