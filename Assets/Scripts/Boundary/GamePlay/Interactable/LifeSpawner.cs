using System;
using System.Collections.Generic;
using Boundary.GamePlay.Enemy.Base;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Interactable
{
    public class LifeSpawner: MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [SerializeField] private List<LifePrefabAssociation> lifePrefabs;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            _eventBus.Subscribe<ELifeDropped>(OnLifeDropped);
        }
        
        private void OnDestroy()
        {
            _eventBus.Unsubscribe<ELifeDropped>(OnLifeDropped);
        }

        #region Event Handlers

        private void OnLifeDropped(ELifeDropped e)
        {
            if (!gameObject.activeSelf)
            {
                Debug.LogWarning("LifeSpawner is inactive, cannot spawn life.");
                return;
            }
            
            var id = e.LifeId;
            
            var lifeObj = Instantiate(GetLifePrefabById(id), e.DropPosition, Quaternion.identity);
            lifeObj.SetLifeId(id);
            lifeObj.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
        
        private Life GetLifePrefabById(LifeId id)
        {
            // Find the prefab associated with the given LifeId
            var association = lifePrefabs.Find(a => a.id == id);
            if (association != null)
            {
                return association.prefab;
            }
            
            Debug.LogError($"No prefab found for LifeId: {id}");
            return null;
        }

        #endregion
    }
    
    [Serializable]
    internal class LifePrefabAssociation
    {
        public LifeId id;
        public Life prefab;
    } 
}