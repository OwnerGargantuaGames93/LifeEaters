using Control.GameData;
using Data.Entities.Game;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Interactable.Levers
{
    public class SingleActivationLever : MonoBehaviour
    {
        [SerializeField] private GameplayEventId gameplayEventId;
        [SerializeField] private GameObject leverOnGameObject;
        [SerializeField] private GameObject leverOffGameObject;

        private IEventBus _eventBus;
        private IGameDataModel _gameData;

        private bool _playerCanPullLever;
        private bool _activated;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
        }

        private void Start()
        {
            _activated = _gameData.HasGameplayEvent(gameplayEventId.ToString());
            UpdateVisuals();
        }

        private void Update()
        {
            if (_activated || !_playerCanPullLever) return;
            if (Commands.UserInput.instance.GeneralInteractWasPressedThisFrame())
                Activate();
        }

        private void Activate()
        {
            _activated = true;
            _eventBus.Publish(new EGameplayEventOccurred(gameplayEventId.ToString()));
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (leverOnGameObject) leverOnGameObject.SetActive(_activated);
            if (leverOffGameObject) leverOffGameObject.SetActive(!_activated);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag)) _playerCanPullLever = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag)) _playerCanPullLever = false;
        }
    }
}
