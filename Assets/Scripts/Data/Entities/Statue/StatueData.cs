using System;
using UnityEngine;

namespace Data.Entities.Statue
{
    [Serializable]
    public class StatueData
    {
        // Persistent Unique ID of the statue
        public string Id { get; }
        
        // Scene that contains the statue
        public int Scene { get; }
        
        // Position of the statue in the scene (will be used to teleport / respawn the player)
        public Vector3 Position { get; }

        // Name of the statue
        public string Name { get; }

        public StatueData(string id, int scene, Vector3 position, string name)
        {
            Id = id;
            Scene = scene;
            Position = position;
            Name = name;
        }
    }
}