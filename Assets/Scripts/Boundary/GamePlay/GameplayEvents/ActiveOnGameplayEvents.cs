using System.Collections.Generic;
using System.Linq;
using Control.GameData;
using Data.Entities.Game;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.GameplayEvents
{
    /// <summary>
    /// Activates or deactivates this GameObject once all the required GameplayEventIds have been collected.
    /// The GameObject must be active in the scene at load time, otherwise Awake never runs:
    /// the initial state is set here based on the saved gameplay events.
    /// </summary>
    public class ActiveOnGameplayEvents : MonoBehaviour
    {
        [SerializeField] private List<GameplayEventId> requiredGameplayEvents;
        [Tooltip("Attivo = il GameObject si attiva quando tutti gli eventi sono stati collezionati (prima è disattivo).\n" +
                 "Disattivo = il GameObject si disattiva quando tutti gli eventi sono stati collezionati (prima è attivo).")]
        [SerializeField] private bool activeWhenCollected = true;

        private IEventBus _eventBus;
        private IGameDataModel _gameData;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;

            if (requiredGameplayEvents is not { Count: > 0 })
            {
                Debug.LogWarning($"[ActiveOnGameplayEvents] No gameplay events configured on '{name}'");
                return;
            }

            // Subscribed in Awake (not OnEnable) so the inactive GameObject keeps listening
            _eventBus.Subscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
            UpdateActiveState();
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
        }

        private void OnGameplayEventOccurred(EGameplayEventOccurred e)
        {
            if (requiredGameplayEvents.All(id => id.ToString() != e.EventId)) return;

            UpdateActiveState();
        }

        private void UpdateActiveState()
        {
            var collected = AllRequiredEventsOccurred();
            gameObject.SetActive(collected == activeWhenCollected);

            if (collected)
            {
                _eventBus.Unsubscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
            }
        }

        private bool AllRequiredEventsOccurred()
        {
            return requiredGameplayEvents.All(id => _gameData.HasGameplayEvent(id.ToString()));
        }
    }
}
