using System.Collections.Generic;
using Control.Player;
using Data.Entities.Effects;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.Pit.Objects.Wool
{
    public class WoolObject : MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [SerializeField] private PitObjectId id;

        private void Awake()
        {
            _eventBus = new EventBus();
        }
    
        public void Use()
        {
            var effectData = new EffectData
            {
                Type = EffectType.BuffFrostResistance,
                Duration = 20f,
                Value = 50f,
                ValueType = EffectValueType.Percentage
            };

            var effects = new List<EffectData> { effectData };
            
            _eventBus.Publish(new EEffectsApplied(effects));
            transform.SetParent(null);
            Destroy(gameObject);
        }
    }
}
