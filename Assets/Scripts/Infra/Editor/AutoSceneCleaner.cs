using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Constants = Utils.Constants;

namespace Infra.Editor
{
    [InitializeOnLoad]
    public class AutoSceneCleaner
    {
        static AutoSceneCleaner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                CleanScenesBeforePlay();
            }
        }

        private static void CleanScenesBeforePlay()
        {
            var sceneCount = SceneManager.sceneCount;

            for (var i = sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.name == Constants.GameplayScene)
                {
                    continue;
                }

                // Chiudi tutto il resto
                EditorSceneManager.CloseScene(scene, false);
            }

            // Se la main scene non è aperta (es. chiudendo tutte le altre l’hai chiusa)
            if (!SceneManager.GetSceneByName(Constants.GameplayScene).isLoaded)
            {
                EditorSceneManager.OpenScene($"Assets/Scenes/{Constants.GameplayScene}.unity", OpenSceneMode.Single);
            }

            Debug.Log("Remove all scenes except Gameplay scene before entering Play Mode."); 
        }
    }
}