using System;
using System.Collections.Generic;
using Infra.EventBus;
using Infra.SceneHandler;
using UnityEngine;

namespace Boundary.Camera
{
    /// <summary>
    /// Lives once in the persistent GamePlay scene alongside every background root it manages.
    /// Backgrounds are never spawned/destroyed per room - they are hand-placed here once and
    /// only ever shown/hidden, so ParallaxLayer's snap-on-enable logic stays the single source
    /// of truth for "where is this layer relative to the camera right now" instead of a second,
    /// parallel positioning mechanism living here.
    ///
    /// Backgrounds are looked up by id via SceneMountConfig (one id per player scene, several
    /// scenes may share the same id) rather than by scene name directly, so scenes that share a
    /// background (e.g. 1.1_parent_house and 1.1_parent_house__clay_crusher) never toggle it off
    /// and back on while transitioning between them.
    /// </summary>
    public class BackgroundManager : MonoBehaviour
    {
        [Serializable]
        public class BackgroundEntry
        {
            public string backgroundId;
            public GameObject root;
        }

        [SerializeField] private SceneMountConfig _sceneMountConfig;
        [SerializeField] private List<BackgroundEntry> _backgrounds = new();

        private IEventBus _eventBus;
        private string _activeBackgroundId;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _eventBus.Subscribe<EPlayerSceneChanged>(OnPlayerSceneChanged);

            // Defensive: every background should already start inactive in the scene file, but
            // make sure none are left on by mistake before the first EPlayerSceneChanged decides
            // which one (if any) should actually be visible.
            foreach (var entry in _backgrounds)
            {
                if (entry.root != null)
                {
                    entry.root.SetActive(false);
                }
            }
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
                Debug.LogWarning($"[BackgroundManager] No SceneMountConfig entry for scene '{e.SceneName}'; leaving background unchanged.");
                return;
            }

            if (backgroundId == _activeBackgroundId)
            {
                // Same background (or same "no background") as before - e.g. moving between
                // 1.1_parent_house and 1.1_parent_house__clay_crusher - so do nothing and avoid
                // any deactivate/reactivate flicker.
                return;
            }

            SetActiveBackground(backgroundId);
        }

        private void SetActiveBackground(string backgroundId)
        {
            var previous = FindBackground(_activeBackgroundId);
            if (previous != null)
            {
                previous.SetActive(false);
            }

            _activeBackgroundId = backgroundId;

            if (string.IsNullOrEmpty(backgroundId))
            {
                // Scene has no background art yet - show nothing rather than guessing.
                return;
            }

            var next = FindBackground(backgroundId);
            if (next == null)
            {
                Debug.LogWarning($"[BackgroundManager] No background root registered for id '{backgroundId}'.");
                return;
            }

            next.SetActive(true);
        }

        private GameObject FindBackground(string backgroundId)
        {
            if (string.IsNullOrEmpty(backgroundId))
            {
                return null;
            }

            var entry = _backgrounds.Find(b => b.backgroundId == backgroundId);
            return entry?.root;
        }
    }
}
