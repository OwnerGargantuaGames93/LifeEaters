using System;
using System.Collections.Generic;
using Data.Database;

namespace Data.Entities.Item
{
    [Serializable]
    public class EssenceItem: Item
    {
        public EssenceId id;
        
        public EssenceType type;

        // TODO: Move to essence version
        public int turnOutEarn;

        public List<PitObjectGeneration> generations;
        
        public EssenceVersion CreateEssenceVersion()
        {
            if (type == EssenceType.Behavioural)
            {
                // Behavioural essences don't generate objects, but still need a version for inventory
                return new EssenceVersion
                {
                    sourceId = id,
                    uniqueId = Guid.NewGuid().ToString(),
                    objects = new List<PitObjectId>() // Empty list for behavioural essences
                };
            }
            
            var version = new EssenceVersion(this);

            // Se l'essenza non produce oggetti, non creare una versione
            return version.objects.Count == 0 ? null : version;
        }
    }
    
    [Serializable]
    public class EssenceVersion
    {
        public string uniqueId;

        public List<PitObjectId> objects = new (); 

        public EssenceId sourceId;
        
        public EssenceVersion(EssenceItem essence) {
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var g in essence.generations)
            {
                // Determinate if the object will be included in this version
                var roll = UnityEngine.Random.Range(0f, 100f);
                if (roll >= g.chance)
                {
                    continue;
                }
                
                // Include the object
                objects.Add(g.id);
            }
            
            sourceId = essence.id;
            uniqueId = Guid.NewGuid().ToString();
        }

        public EssenceVersion()
        {
        }

        public float GetTotalEnergyCost()
        {
            var totalCost = 0f;
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var objId in objects)
            {
                var objData = DataSource.Instance.GetPitObject(objId);
                totalCost += objData.energyCost;
            }

            return totalCost;
        }
    }
    
    [Serializable]
    public struct PitObjectGeneration
    {
        public PitObjectId id;
        public float chance;
    }
    
    public enum EssenceType
    {
        Standard,
        Behavioural
    }
    
    public enum EssenceId
    {
        Empty,
        Shepherd,
        Miner,
        Mechanical,
        SectSoldier1,
        Patient,
        LazyPerson,
    }
}