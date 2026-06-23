using Boundary.Commands;
using Boundary.Utils;
using Control.DialogueHandler;
using Control.Quests;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Interactable
{
    public class Npc: IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        private IDialogueHandler _dialogueHandler;
        private IQuestModel _questModel;
        
        [SerializeField] private string npcName;
        [SerializeField] private string npcGameInSceneId;
        
        private bool _playerInRange;
        private bool _isVisible = true;

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;
            _dialogueHandler = GameContext.Instance.DialogueHandler;
            _questModel = GameContext.Instance.Quests;
            
            SubscribeToEvents();
        }
        
        private void Start()
        {
            // Check if this NPC should be visible based on current quest phase
            // This runs when the scene loads (game start or scene change)
            UpdateVisibility();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void OnPlayerRestOnStatue(EPlayerRestOnStatue e)
        {
            // When player rests, update NPC based on current quest phase
            UpdateVisibility();
        }
        
        private void UpdateVisibility()
        {
            var currentPhase = _questModel.GetNpcCurrentPhase(npcName);
            
            if (currentPhase == null)
            {
                // No quest data found, keep NPC visible by default
                SetVisible(true);
                Debug.LogWarning($"[Npc] '{npcName}' (Id: {Id}) has no quest data, staying visible by default");
                return;
            }
            
            // Check if this NPC GameObject should be visible
            // NPC is visible if the npcGameObjectId matches this GameObject's Id
            var shouldBeVisible = currentPhase.npcGameObjectId == npcGameInSceneId;
            
            SetVisible(shouldBeVisible);
            
            // Debug.Log($"[Npc] '{npcName}' (Id: {Id}) visibility set to {shouldBeVisible} " +
            //          $"(phase: '{currentPhase.phaseName}', expected Id: '{currentPhase.npcGameObjectId}')");
        }
        
        private void SetVisible(bool visible)
        {
            _isVisible = visible;
            gameObject.SetActive(visible);
        }

        private void Update()
        {
            if (!_isVisible)
            {
                return;
            }
            
            if (_playerInRange && UserInput.instance.NpcTalkWasPressedThisFrame() && !_dialogueHandler.IsDialoguePlaying())
            {
                _eventBus.Publish(new EEnterDialogue(npcName));
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerInRange = true;
            }  
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                _playerInRange = false;
            }  
        }
    }
}