using System;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Infra.TilemapController
{
    public class TilemapController: MonoBehaviour
    {
        private IEventBus _eventBus;
        
        public static TilemapController Instance { get; private set; }
        
        public Tilemap ground;
        public Tilemap linearGround;
        public Tilemap wall;
        public Tilemap oneWayPlatform;
        public Tilemap twoWayPlatform;
        public Tilemap ladder;

        private GameObject[] _standardHiddenWallsCurrentlyHidden = new GameObject[0];
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _eventBus = GameContext.Instance.EventBus;

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EHideStandardHiddenWalls>(OnHideStandardHiddenWalls);
            _eventBus.Subscribe<EShowStandardHiddenWalls>(OnShowStandardHiddenWalls);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EHideStandardHiddenWalls>(OnHideStandardHiddenWalls);
            _eventBus.Unsubscribe<EShowStandardHiddenWalls>(OnShowStandardHiddenWalls);
        }
        
        private void OnHideStandardHiddenWalls(EHideStandardHiddenWalls e)
        {
            _standardHiddenWallsCurrentlyHidden = Array.Empty<GameObject>();
            _standardHiddenWallsCurrentlyHidden = GameObject.FindGameObjectsWithTag(Constants.StandardHiddenWallTag);
            
            foreach (var w in _standardHiddenWallsCurrentlyHidden)
            {
                w.SetActive(false);
            }
        }

        private void OnShowStandardHiddenWalls(EShowStandardHiddenWalls e)
        {
            foreach (var w in _standardHiddenWallsCurrentlyHidden)
            {
                w.SetActive(true);
            }
            
            _standardHiddenWallsCurrentlyHidden = Array.Empty<GameObject>();
        }
    }
    
    #region Events

    public struct EHideStandardHiddenWalls
    {
        
    }

    public struct EShowStandardHiddenWalls
    {
        
    }
    
    #endregion
}