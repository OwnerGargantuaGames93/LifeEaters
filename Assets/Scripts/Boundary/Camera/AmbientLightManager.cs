using System;
using System.Collections;
using System.Collections.Generic;
using Infra.EventBus;
using Infra.SceneHandler;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Boundary.Camera
{
    /// <summary>
    /// Lives once in the persistent GamePlay scene on the single global Light2D that lights the
    /// external layers (Background_*, *_External). Room scenes must NOT carry their own global
    /// light on those layers: when several rooms are loaded additively URP only accepts one
    /// global light per sorting layer and blend style.
    ///
    /// Color and intensity are looked up by backgroundId via SceneMountConfig, same as
    /// BackgroundManager, so scenes sharing a background share the same ambient light and never
    /// re-trigger the transition while moving between them.
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class AmbientLightManager : MonoBehaviour
    {
        [Serializable]
        public class AmbientLightEntry
        {
            public string backgroundId;
            public Color color = Color.white;
            public float intensity = 1f;
        }

        [SerializeField] private Light2D _light2D;
        [SerializeField] private SceneMountConfig _sceneMountConfig;
        [SerializeField] private List<AmbientLightEntry> _lights = new();

        [Header("Default (scene senza backgroundId o senza entry)")]
        [SerializeField] private Color _defaultColor = Color.white;
        [SerializeField] private float _defaultIntensity = 1f;

        [Tooltip("Durata della transizione in secondi (0 = cambio istantaneo)")]
        [SerializeField, Min(0f)] private float _transitionDuration;

        private IEventBus _eventBus;
        private string _activeBackgroundId;
        private Coroutine _transition;

        private void Reset()
        {
            _light2D = GetComponent<Light2D>();
        }

        private void Awake()
        {
            if (_light2D == null) _light2D = GetComponent<Light2D>();

            _eventBus = GameContext.Instance.EventBus;
            _eventBus.Subscribe<EPlayerSceneChanged>(OnPlayerSceneChanged);

            // Until the first EPlayerSceneChanged tells us where the player is
            _light2D.color = _defaultColor;
            _light2D.intensity = _defaultIntensity;
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<EPlayerSceneChanged>(OnPlayerSceneChanged);
        }

        private void OnPlayerSceneChanged(EPlayerSceneChanged e)
        {
            var hasEntry = _sceneMountConfig.TryGetBackgroundId(e.SceneName, out var backgroundId);

            if (!hasEntry)
            {
                Debug.LogWarning($"[AmbientLightManager] No SceneMountConfig entry for scene '{e.SceneName}'; leaving ambient light unchanged.");
                return;
            }

            if (backgroundId == _activeBackgroundId)
            {
                // Same background as before (e.g. 1.1_parent_house -> 1.1_parent_house__clay_crusher)
                return;
            }

            _activeBackgroundId = backgroundId;
            ApplyLight(backgroundId);
        }

        private void ApplyLight(string backgroundId)
        {
            var entry = string.IsNullOrEmpty(backgroundId)
                ? null
                : _lights.Find(l => l.backgroundId == backgroundId);

            var color = entry?.color ?? _defaultColor;
            var intensity = entry?.intensity ?? _defaultIntensity;

            if (_transition != null)
            {
                StopCoroutine(_transition);
                _transition = null;
            }

            // An inactive GameObject can't run coroutines: apply the values immediately
            if (_transitionDuration <= 0f || !isActiveAndEnabled)
            {
                _light2D.color = color;
                _light2D.intensity = intensity;
                return;
            }

            _transition = StartCoroutine(TransitionTo(color, intensity));
        }

        private IEnumerator TransitionTo(Color color, float intensity)
        {
            var startColor = _light2D.color;
            var startIntensity = _light2D.intensity;
            var elapsed = 0f;

            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / _transitionDuration;
                _light2D.color = Color.Lerp(startColor, color, t);
                _light2D.intensity = Mathf.Lerp(startIntensity, intensity, t);
                yield return null;
            }

            _light2D.color = color;
            _light2D.intensity = intensity;
            _transition = null;
        }
    }
}
