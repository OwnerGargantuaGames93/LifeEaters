using System;
using System.Collections.Generic;
using System.Linq;
using Boundary.Commands;
using Control.Inventory;
using Control.Player;
using Data.Entities.Item;
using Infra.EventBus;
using Infra.TilemapController;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

namespace Boundary.Pit
{
    public class PitSpawner : MonoBehaviour
    {
        private const string PitTagName = "Pit";

        private IEventBus _eventBus;
        private IPlayerModel _player;
        private IInventoryModel _inventory;
        
        private GameObject _pit;

        private Tilemap _groundTileMap;
        private Tilemap _linearGroundTileMap;
        private Tilemap _oneWayTileMap;
        private Tilemap _twoWayTileMap;

        private List<(Vector3, string)> _availablePlaces;
        private Collider2D _pitCollider;

        private UnityEngine.Camera _camera;
        private Transform _playerTransform;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _player = GameContext.Instance.Player;
            _inventory = GameContext.Instance.Inventory;
        }

        private void Start()
        {
            _pit = GameObject.FindGameObjectWithTag(PitTagName);
            _pitCollider = _pit.GetComponent<Collider2D>();
            _availablePlaces = new List<(Vector3, string)>();

            _groundTileMap = TilemapController.Instance.ground;
            _oneWayTileMap = TilemapController.Instance.oneWayPlatform;
            _twoWayTileMap = TilemapController.Instance.twoWayPlatform;
            _linearGroundTileMap = TilemapController.Instance.linearGround;
            
            _camera = UnityEngine.Camera.main;
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private List<(Vector3, string)> FindLocationsOfTiles(Tilemap tileMap, string tileName, Vector3Int minCell, Vector3Int maxCell)
        {
            // create a new list of vectors by doing...
            var places = new List<(Vector3, string)>();

            for (var n = minCell.x; n <= maxCell.x; n++)
            {
                for (var p = minCell.y; p <= maxCell.y; p++)
                {
                    var localPlace = new Vector3Int(n, p, (int)tileMap.transform.position.y);
                    var place = tileMap.CellToWorld(localPlace);

                    if (tileMap.HasTile(localPlace) && IsVisibleInCamera(place))
                    {
                        var abovePlace = new Vector3Int(n, p + 1, (int)tileMap.transform.position.y);
                        if (tileMap.HasTile(abovePlace))
                        {
                            continue;
                        }
                            
                        places.Add((place, tileName));
                    }
                }
            }
            
            return places;
        }
        
        private void RefreshAvailablePlaces()
        {
            if (!_groundTileMap || !_oneWayTileMap || !_linearGroundTileMap)
            {
                throw new PitSpawnerException("Some tilemaps are not assigned in PitSpawner.");
            }
            
            
            if (!_camera)
            {
                throw new PitSpawnerException("Main camera not found.");
            }
            
            var camHeight = 2f * _camera.orthographicSize;
            var camWidth = camHeight * _camera.aspect;
            
            // Check if LazyPerson behavioral essence is equipped
            var isLazyPersonEquipped = _inventory.EquippedBehaviouralEssences()
                .Any(e => e.item.sourceId == EssenceId.LazyPerson);
            
            // Reduce spawn area by 2 tiles (in world units) if LazyPerson is equipped
            var spawnReduction = isLazyPersonEquipped ? 2f : 0f;

            var camPos = _camera.transform.position;
            var centerPos = isLazyPersonEquipped ? _playerTransform.position : camPos;
            var minWorld = new Vector3(centerPos.x - camWidth / 2 + spawnReduction, centerPos.y - camHeight / 2 + spawnReduction, 0);
            var maxWorld = new Vector3(centerPos.x + camWidth / 2 - spawnReduction, centerPos.y + camHeight / 2 - spawnReduction, 0);

            // Convert world bounds to tilemap cell coordinates
            var groundTilemapMinCell = _groundTileMap.WorldToCell(minWorld);
            var groundTilemapMaxCell = _groundTileMap.WorldToCell(maxWorld);
            var oneWayTilemapMinCell = _oneWayTileMap.WorldToCell(minWorld);
            var oneWayTilemapMaxCell = _oneWayTileMap.WorldToCell(maxWorld);
            var twoWayTilemapMinCell = _twoWayTileMap.WorldToCell(minWorld);
            var twoWayTilemapMaxCell = _twoWayTileMap.WorldToCell(maxWorld);
            var linearGroundTilemapMinCell = _linearGroundTileMap.WorldToCell(minWorld);
            var linearGroundTilemapMaxCell = _linearGroundTileMap.WorldToCell(maxWorld);
            
            _availablePlaces.Clear();
            _availablePlaces.AddRange(FindLocationsOfTiles(_groundTileMap, "ground", groundTilemapMinCell, groundTilemapMaxCell));
            _availablePlaces.AddRange(FindLocationsOfTiles(_oneWayTileMap, "oneway", oneWayTilemapMinCell, oneWayTilemapMaxCell));
            _availablePlaces.AddRange(FindLocationsOfTiles(_linearGroundTileMap, "linear_ground", linearGroundTilemapMinCell, linearGroundTilemapMaxCell));
            _availablePlaces.AddRange(FindLocationsOfTiles(_twoWayTileMap, "twoway", twoWayTilemapMinCell, twoWayTilemapMaxCell));
        }

        private void Update()
        {
            // Only proceed if the user wants to generate a pit and is allowed
            if (!UserInput.instance.PitGenerateWasPressedThisFrame() || !_player.CanGeneratePit())
            {
                return;
            }

            // Refresh available places only when needed
            RefreshAvailablePlaces();
            SpawnPit();
        }

        private bool IsVisibleInCamera(Vector3 pos)
        {
            var bounds = new Bounds(pos, _pitCollider.bounds.size);
            var cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(UnityEngine.Camera.main);
            return GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, bounds);
        }

        private void SpawnPit()
        {
            var numberOfPits = _player.GetNumberOfPitsCanBeGenerated();
            
            var randomPoints = GetRandomPoints(numberOfPits);
        
            for (var i = 0; i < randomPoints.Count; i++)
            {
                var item = randomPoints[i];

                switch (item.Item2)
                {
                    // TODO: Different pit types for different tile types (for now they are the same)
                    case "ground":
                    case "oneway":
                    case "twoway":
                    case "linear_ground":
                    {
                        var pitObject = Instantiate(_pit, new Vector3(randomPoints[i].Item1.x + 0.5f, randomPoints[i].Item1.y + 1.25f, randomPoints[i].Item1.z), Quaternion.identity);
                        pitObject.GetComponent<Pit>().SetIsDestroyable(true);
                        break;
                    }
                }
            }
            
            _eventBus.Publish(new EPitsGenerated());
        }

        
        private List<(Vector3, string)> GetRandomPoints(int count)
        {
            var outputList = new List<(Vector3, string)>();

            if (_availablePlaces.Count == 0)
            {
                return outputList;
            }
            
            for (var i = 0; i < count; i++)
            {
                var index = Random.Range(0, _availablePlaces.Count);
                
                outputList.Add((_availablePlaces[index].Item1, _availablePlaces[index].Item2));
                
                // Avoid to select same index in randomization
                _availablePlaces.RemoveAt(index);
            }

            return outputList;
        }

    }
    
    #region Events
    
    public struct EPitsGenerated
    {
    }
    
    #endregion
    
    #region Exceptions
    
    public class PitSpawnerException : Exception
    {
        public PitSpawnerException(string message)
            : base(message) { }
    }
    
    #endregion
}