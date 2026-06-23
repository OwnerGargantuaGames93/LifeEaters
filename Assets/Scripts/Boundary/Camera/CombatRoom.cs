using System;
using System.Linq;
using Boundary.GamePlay.Enemy.Base;
using Boundary.Pit;
using Boundary.Utils;
using Control.GameData;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Camera
{
    // Le combat room sono stanze in cui il personaggio deve sconfiggere tutti i nemici senza morire
    // Nelle combat room si attivano muri invisibili che impediscono al personaggio di uscire dalla stanza finché tutti i nemici non sono sconfitti
    // Se il personaggio muore, i nemici si resettano e il personaggio viene respawnato nell'ultimo respawn point (fuori dalla room)
    // Visto che non c'è nessun respawn point all'interno della room, la logica di reset può essere gestita semplicemente resettando i nemici quando il personaggio esce dalla room
    public class CombatRoom : IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        private IGameDataModel _gameData;

        public GameObject virtualCamera;

        private CombatRoomState _state = CombatRoomState.Inactive;
        private GameObject _player;
        private Collider2D _playerCollider;
        private PolygonCollider2D _roomCollider;
        private bool _isPlayerNearRoom;
        private bool _cameraActive;
        private bool _playerInsideRoomBuffer;

        [SerializeField] private BaseEnemy[] enemies;
        [SerializeField] private BoxCollider2D[] walls = Array.Empty<BoxCollider2D>();
        [SerializeField] private string roomName;

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;

            _player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
            _playerCollider = _player.GetComponent<BoxCollider2D>();
            _roomCollider = GetComponent<PolygonCollider2D>();

            foreach (var enemy in enemies)
            {
                enemy.SetComa(true);
            }
            
            roomName ??= gameObject.name;
        }

        private void Start()
        {
            var isRoomCleared = _gameData.IsCombatRoomCleared(Id);
            
            if (isRoomCleared)
            {
               _state = CombatRoomState.Cleared;
               
               foreach (var enemy in enemies)
               {
                   enemy.gameObject.SetActive(false);
               }
               
               OpenRoom();
            }
        }

        private void ResetRoomOnExit()
        {
            
            OpenRoom();
            _state = CombatRoomState.Inactive;
                
            // reset enemies
            foreach (var enemy in enemies)
            {
                enemy.Initialize(false);
                enemy.SetComa(true);
            }
        }

        private void Update()
        {
            var playerInRoom  = _isPlayerNearRoom && IsPlayerFullyInsideRoom();
            
            if (playerInRoom)
            {
                if (!_cameraActive)
                {
                    virtualCamera.SetActive(true);
                    _cameraActive = true;
                    _eventBus.Publish(new EPlayerEnteredCombatRoom(Id, _state));
                }
                
                if (!_playerInsideRoomBuffer)
                {
                    _playerInsideRoomBuffer = true;
                   
                    // activate enemies
                    foreach (var enemy in enemies) 
                    {
                        enemy.SetComa(false);
                    }

                    if (_state == CombatRoomState.Inactive)
                    {
                        CloseRoom();
                        _state = CombatRoomState.Active;
                    }
                }
            }
            
            if (!playerInRoom)
            {
                if (_cameraActive)
                {
                    virtualCamera.SetActive(false);
                    _cameraActive = false;
                    
                    _eventBus.Publish(new EPlayerExitedCombatRoom(Id));
                }
                
                if (_playerInsideRoomBuffer)
                {
                    _playerInsideRoomBuffer = false;

                    // If the player exits the room while it's active (aka is dead or use a teleport item)
                    // reset the room
                    if (_state == CombatRoomState.Active)
                    {
                        ResetRoomOnExit();    
                    }
                }
            }
            
            // Check if all enemies are dead and the room is active
            if (_state == CombatRoomState.Active && enemies.All(e => e.IsDead()))
            {
                _eventBus.Publish(new ECombatRoomCompleted(Id));
                _state = CombatRoomState.Cleared;
                OpenRoom();
            }
            
            _playerInsideRoomBuffer = playerInRoom;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _isPlayerNearRoom = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _isPlayerNearRoom = false;
            }
        }

        private void CloseRoom()
        {
            foreach (var wall in walls)
            {
                wall.enabled = true;
            }
        }

        private void OpenRoom()
        {
            foreach (var wall in walls)
            {
                wall.enabled = false;
            }
        }

        private bool IsPlayerFullyInsideRoom()
        {
            var bounds = _playerCollider.bounds;
            var room = _roomCollider;
            if (room is null)
            {
                return false;
            }

            var min = bounds.min;
            var max = bounds.max;
            var corners = new Vector2[]
            {
                min,
                new(min.x, max.y),
                new(max.x, min.y),
                max
            };

            foreach (var corner in corners)
            {
                if (!room.OverlapPoint(corner))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public enum CombatRoomState
    {
        Inactive,
        Active,
        Cleared
    }

    #region Events
    
    public struct EPlayerEnteredCombatRoom
    {
        public string RoomId;
        public CombatRoomState RoomState;
        
        public EPlayerEnteredCombatRoom(string roomId, CombatRoomState roomState)
        {
            RoomId = roomId;
            RoomState = roomState;
        }
    }
    
    public struct EPlayerExitedCombatRoom
    {
        public string RoomId;
        
        public EPlayerExitedCombatRoom(string roomId)
        {
            RoomId = roomId;
        }
    }

    public struct ECombatRoomCompleted
    {
        public readonly string RoomId;
        
        public ECombatRoomCompleted(string roomId)
        {
            RoomId = roomId;
        }
    }
    
    #endregion
}