using System.Collections;
using Control.GameData;
using Data.Entities.Game;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Boundary.Lights
{
    /// <summary>
    /// Changes the color of the Light2D on this GameObject when the configured GameplayEventId occurs.
    /// If the event already occurred (loaded from save), the color is applied immediately on Start.
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class Light2DColorOnGameplayEvent : MonoBehaviour
    {
        private Light2D light2D;
        [SerializeField] private GameplayEventId gameplayEventId;

        [Header("Colore")]
        [SerializeField] private Color targetColor = Color.white;
        [Tooltip("Durata della transizione in secondi (0 = cambio istantaneo)")]
        [SerializeField, Min(0f)] private float transitionDuration;

        private IEventBus _eventBus;
        private IGameDataModel _gameData;

        private void Reset()
        {
            light2D = GetComponent<Light2D>();
        }

        private void Awake()
        {
            if (light2D == null) light2D = GetComponent<Light2D>();

            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
        }

        private void Start()
        {
            if (gameplayEventId == GameplayEventId.None)
            {
                Debug.LogWarning($"[Light2DColorOnGameplayEvent] No gameplay event configured on '{name}'");
                return;
            }

            // Event already happened in a previous session: no transition
            if (_gameData.HasGameplayEvent(gameplayEventId.ToString()))
            {
                light2D.color = targetColor;
                return;
            }

            _eventBus.Subscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
        }

        private void OnGameplayEventOccurred(EGameplayEventOccurred e)
        {
            if (e.EventId != gameplayEventId.ToString()) return;

            _eventBus.Unsubscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);

            // An inactive GameObject can't run coroutines: apply the color immediately
            if (transitionDuration <= 0f || !isActiveAndEnabled)
            {
                light2D.color = targetColor;
                return;
            }

            StartCoroutine(TransitionTo(targetColor));
        }

        private IEnumerator TransitionTo(Color color)
        {
            var startColor = light2D.color;
            var elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                light2D.color = Color.Lerp(startColor, color, elapsed / transitionDuration);
                yield return null;
            }

            light2D.color = color;
        }
    }
}
