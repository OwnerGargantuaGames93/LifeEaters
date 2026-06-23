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
        }
        
        public List<SceneMountEntry> sceneMap = new();
        
        public HashSet<string> GetRequiredScenes(string sceneName)
        {
            var entry = sceneMap.Find(e => e.scene == sceneName);
            return entry != null ? new HashSet<string>(entry.requiredScenes) : new HashSet<string>();
        }
    }
}