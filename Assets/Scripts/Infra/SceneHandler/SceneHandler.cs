using System.Collections.Generic;
using Boundary.Player;
using Control.GameData;
using Data.Entities.Game;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Infra.SceneHandler
{
    public class SceneHandler: MonoBehaviour, ISceneHandler
    {
        private IEventBus _eventBus;
        private string _currentPlayerScene;

        [SerializeField] private SceneMountConfig _sceneMountConfig;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;

            SubscribeToEvents();            
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EGameDataLoaded>(OnGameLoaded);
            _eventBus.Subscribe<EPlayerEnterScene>(OnPlayerEnterScene);
        }
        
        #region Event Handlers
 
        // Handle game loaded event
        // For example, initialize world state based on loaded data
        private void OnGameLoaded(EGameDataLoaded e)
        {
            var data = e.GameData;
            
            // Unload the current scene except for the GamePlay scene
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name != Constants.GameplayScene && scene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }

            _currentPlayerScene = data.sceneName;
            
            // TODO: Verify if is correct to place this event shot here!!! 
            _eventBus.Publish(new EPlayerSceneChanged(_currentPlayerScene));

            Debug.Log("[SceneHandler] Game loaded. Player scene: " + _currentPlayerScene);
            
            var requiredScenes = _sceneMountConfig.GetRequiredScenes(_currentPlayerScene);
            
            Debug.Log($"[SceneHandler] Loading scenes for player scene '{_currentPlayerScene}': {string.Join(", ", requiredScenes)}");
            
            foreach (var sceneName in requiredScenes)
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            }
            
            // Route through the same respawn flow as a statue teleport, instead of a raw
            // transform set, so velocity resets, the camera warps instantly and every system
            // that reacts to a teleport (e.g. ParallaxLayer) gets its EPlayerTeleported hook.
            _eventBus.Publish(new ETeleportPlayerToPosition(data.initialPosition));
        }

        private void OnPlayerEnterScene(EPlayerEnterScene e)
        {
            if (e.SceneName == _currentPlayerScene)
            {
                return; // No scene change
            }

            _currentPlayerScene = e.SceneName;
            _eventBus.Publish(new EPlayerSceneChanged(_currentPlayerScene));
            var requiredScenes = _sceneMountConfig.GetRequiredScenes(_currentPlayerScene);
            
            Debug.Log($"[SceneHandler] Loading scenes for player scene '{_currentPlayerScene}': {string.Join(", ", requiredScenes)}");
            
            // Iterate all the active scenes:
            // - Load the required scenes that are not loaded yet
            // - Unload the scenes that are not required anymore
            var loadedScenes = new HashSet<string>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name == Constants.GameplayScene)
                {
                    continue;
                }
                
                loadedScenes.Add(scene.name);
                
                if (!requiredScenes.Contains(scene.name))
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
            
            foreach (var sceneName in requiredScenes)
            {
                if (!loadedScenes.Contains(sceneName))
                {
                    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                }
            }
        }
        
        #endregion

        #region Utility
        #endregion

        #region Interface Implementation

        public void Save(GameSessionData gameData)
        {
            gameData.sceneName = _currentPlayerScene;
        }

        #endregion
    }

    #region Events

    public struct EPlayerEnterScene
    {
        public readonly string SceneName;

        public EPlayerEnterScene(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    public struct EPlayerSceneChanged
    {
        public readonly string SceneName;

        public EPlayerSceneChanged(string sceneName)
        {
            SceneName = sceneName;
        }
    }

    #endregion
}