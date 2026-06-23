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
        }

        private void Start()
        {
            _data = new StatueData(
                Id,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle,
                gameObject.transform.position,
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