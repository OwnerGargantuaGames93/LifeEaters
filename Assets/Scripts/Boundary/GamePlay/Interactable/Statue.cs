using Boundary.Commands;
using Boundary.Utils;
using Control.GameData;
using Data.Entities.Statue;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Interactable
{
    public class Statue : IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        
        // Name of the statue, configurable in the Unity Editor
        [SerializeField] private string statueName;
        
        // Point where the player respawns / is teleported (load game, return to statue).
        // If not assigned, falls back to a child named "SpawnPoint", then to the statue's own transform.
        [SerializeField] private Transform spawnPoint;
        
        private const string SpawnPointChildName = "SpawnPoint";
        
        // Data for the statue, including its name, position, and unique ID
        // This will store in gameplay data when the statue will be activated
        private StatueData _data;
        
        // Flag to check if the statue is already activated
        // private bool _isActive;
        
        private bool _playerOnStatue;

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;

            if (spawnPoint == null)
            {
                spawnPoint = transform.Find(SpawnPointChildName);
            }

            if (spawnPoint == null)
            {
                Debug.LogWarning($"[Statue] No spawn point on '{statueName}', using the statue position.");
                spawnPoint = transform;
            }
        }

        private void Start()
        {
            _data = new StatueData(
                Id,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle,
                spawnPoint.position,
                statueName
            );
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.PlayerTag))
            {
                return;
            }

            _playerOnStatue = true;
            
            _eventBus.Publish(new EPlayerActivateStatue(_data));
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerOnStatue = false;
            }
        }
        
        private void Update()
        {
            if (!_playerOnStatue)
            {
                return;
            }

            if (UserInput.instance.OpenStatueMenuWasPressedThisFrame())
            {
                _eventBus.Publish(new EPlayerRestOnStatue(_data));
            }
        }
    }
    
    #region Events
    
    public struct EPlayerActivateStatue
    {
        public readonly StatueData StatueData;
        
        public EPlayerActivateStatue(StatueData statueData)
        {
            StatueData = statueData;
        }
    }
    
    public struct EPlayerRestOnStatue
    {
        public readonly StatueData StatueData;
        
        public EPlayerRestOnStatue(StatueData statueData)
        {
            StatueData = statueData;
        }
    }
    
    #endregion
}