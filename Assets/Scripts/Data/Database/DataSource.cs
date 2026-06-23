using System.Collections.Generic;
using System.Linq;
using Data.Database.Sources.Consumables;
using Data.Database.Sources.Equipments;
using Data.Database.Sources.Essences;
using Data.Database.Sources.Keys;
using Data.Database.Sources.Lifes;
using Data.Database.Sources.PitObjects;
using Data.Database.Sources.Shops;
using Data.Database.Sources.Talents;
using Data.Entities.Item;
using Data.Entities.Shops;
using UnityEngine;

namespace Data.Database
{
    public class DataSource: MonoBehaviour
    {
        public static DataSource Instance;

        // MARK: - Repositories
        private Dictionary<PitObjectId, PitObjectData> PitObjectsDictionary;
        private Dictionary<EssenceId, EssenceItem> EssenceDictionary;
        private Dictionary<TalentId, TalentItem> TalentsDictionary;
        private Dictionary<ConsumableId, ConsumableItem> ConsumablesDictionary;
        private Dictionary<EquipmentId, EquipmentItem> EquipmentDictionary;
        private Dictionary<KeyId, KeyItem> KeysDictionary;
        private Dictionary<LifeId, LifeItem> LifesDictionary;
        private Dictionary<string, Shop> ShopsDictionary;

        // MARK: - Databases
        [SerializeField] public PitObjectDatabase _PitObjectDatabase;
        [SerializeField] public EssenceDatabase _EssenceDatabase;
        [SerializeField] public TalentDatabase _TalentDatabase;
        [SerializeField] public ConsumableDatabase _ConsumableDatabase;
        [SerializeField] public EquipmentDatabase _EquipmentDatabase;
        [SerializeField] public KeyDatabase _KeyDatabase;
        [SerializeField] public LifeDatabase _LifeDatabase;
        [SerializeField] public ShopDatabase _ShopDatabase;

        private void Awake()
        {
            Instance = this;

            PitObjectsDictionary = _PitObjectDatabase.Items.ToDictionary(i => i.id);
            EssenceDictionary = _EssenceDatabase.items.ToDictionary(i => i.id);
            TalentsDictionary = _TalentDatabase.Items.ToDictionary(i => i.Id);
            ConsumablesDictionary = _ConsumableDatabase.Items.ToDictionary(i => i.Id);
            EquipmentDictionary = _EquipmentDatabase.items.ToDictionary(i => i.id);
            KeysDictionary = _KeyDatabase.Items.ToDictionary(i => i.Id);
            LifesDictionary = _LifeDatabase.items.ToDictionary(i => i.id);
            ShopsDictionary = _ShopDatabase.shops.ToDictionary(s => s.shopName);
        }
        
        // MARK: - Query Methods
        public PitObjectData GetPitObject(PitObjectId id) => PitObjectsDictionary.TryGetValue(id, out var item) ? item : null;
        
        public EssenceItem GetEssenceItem(EssenceId id) => EssenceDictionary.TryGetValue(id, out var item) ? item : null;
        
        public TalentItem GetTalentItem(TalentId id) => TalentsDictionary.TryGetValue(id, out var item) ? item : null;
        
        public ConsumableItem GetConsumableItem(ConsumableId id) => ConsumablesDictionary.TryGetValue(id, out var item) ? item : null;
        
        public EquipmentItem GetEquipmentItem(EquipmentId id) => EquipmentDictionary.TryGetValue(id, out var item) ? item : null;
        
        public KeyItem GetKeyItem(KeyId id) => KeysDictionary.TryGetValue(id, out var item) ? item : null;
        
        public LifeItem GetLifeItem(LifeId id) => LifesDictionary.TryGetValue(id, out var item) ? item : null;
        
        public Shop GetShopByName(string shopName) => ShopsDictionary.TryGetValue(shopName, out var shop) ? shop : null;
    }
}