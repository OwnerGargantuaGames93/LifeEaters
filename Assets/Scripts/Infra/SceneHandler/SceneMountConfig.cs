using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra.SceneHandler
{
    [CreateAssetMenu(fileName = "SceneMountConfig", menuName = "Game/Scene Mount Config")]
    public class SceneMountConfig: ScriptableObject
    {
        [Serializable]
        public class SceneMountEntry
        {
            public string scene;
            public List<string> requiredScenes;

            [Tooltip("Id of the persistent background (see BackgroundManager) to show while " +
                     "this is the current player scene. Leave empty if this scene has no " +
                     "background art yet - BackgroundManager will hide any active background.")]
            public string backgroundId;
        }

        public List<SceneMountEntry> sceneMap = new();

        public HashSet<string> GetRequiredScenes(string sceneName)
        {
            var entry = sceneMap.Find(e => e.scene == sceneName);
            return entry != null ? new HashSet<string>(entry.requiredScenes) : new HashSet<string>();
        }

        public bool TryGetBackgroundId(string sceneName, out string backgroundId)
        {
            var entry = sceneMap.Find(e => e.scene == sceneName);
            backgroundId = entry?.backgroundId;
            return entry != null;
        }
    }
}