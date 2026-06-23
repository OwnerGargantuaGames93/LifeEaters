using System.Collections.Generic;
using System.Linq;
using Boundary.Camera;
using Boundary.Collectibles;
using Boundary.Enemy.Behaviors.Chase.Shooting;
using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Interactable;
using Boundary.GamePlay.Projectile;
using Boundary.Interactable;
using Boundary.Pit;
using Boundary.UI.Shop;
using Control.Effects;
using Control.GameData;
using Control.Inventory;
using Control.Pit;
using Control.Player.UseCase;
using Control.Shop;
using Data.Damage;
using Data.Database;
using Data.Entities.Effects;
using Data.Entities.Item;
using Data.Entities.Player;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Control.Player
{
    /// <summary>
    /// Player data model. Contains player-related data such as status, attributes,
    /// characteristics and the logic to manipulate them.
    /// </summary>
    public class PlayerModel: IPlayerModel
    {
        // Service dependencies
        private readonly IEventBus _eventBus;
        private readonly EffectCalculator _effectCalculator;
        private readonly IPitModel _pitModel;

        #region Public Getters

        // Currently player talents acquired
        private List<TalentId> Talents { get; set; } = new();
        // Player talents acquired by equipment and other temporary sources
        private List<TalentId> AdditionalTalents { get; set; } = new();
        // Player characteristics acquired by leveling up
        public PlayerCharacteristics Characteristics { get; private set; }
        // Player characteristics that depends on
        // equipment and other temporary sources
        public PlayerCharacteristics AdditionalCharacteristics { get; private set; }
        // Player attributes that depends on player characteristics
        // This will be calculated from characteristics
        public PlayerAttributes Attributes { get; private set; }
        // Player attributes that not depends on player characteristics
        // This will take into account temporary buffs / debuffs
        // like some equipment and other temporary sources
        public PlayerAttributes AdditionalAttributes { get; private set; }
        // Player status (current health, energy, lives)
        // This will take into account attributes derived from characteristics
        // and additional attributes
        public PlayerStatus Status { get; private set; }
        // Effects to be applied when player death (aka lose 1 life)
        public List<EffectData> EffectsOnDeath { get; set; } = new();

        #endregion

        #region Utility Getters

        private PlayerCharacteristics AllCharacteristics => Characteristics + AdditionalCharacteristics;
        private List<TalentId> AllTalents => Talents.Concat(AdditionalTalents).ToList();
        private PlayerAttributes AllAttributes => Attributes + AdditionalAttributes;

        #endregion
        
        #region Gameplay References
        private bool _inCombatRoom;
        private Vector3 _playerPosition;
        #endregion
        
        public PlayerModel(IEventBus eventBus, IPitModel pitModel)
        {
            _eventBus = eventBus;
            _pitModel = pitModel;

            _effectCalculator = new EffectCalculator(_eventBus, this);

            SubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EItemsEquipped>(OnItemsEquipped);
            _eventBus.Subscribe<EItemsUnequipped>(OnItemsUnequipped);
            _eventBus.Subscribe<EPlayerReceiveDamage>(OnPlayerReceiveDamage);
            _eventBus.Subscribe<EPlayerReceiveContactDamageByEnemy>(OnPlayerReceiveContactDamage);
            _eventBus.Subscribe<EEnemyBulletHitPlayer>(OnEnemyBulletHitPlayer);
            _eventBus.Subscribe<EPoisonEffectFinished>(OnStopPoisonEffect);
            _eventBus.Subscribe<EBurnEffectFinished>(OnStopBurntEffect);
            _eventBus.Subscribe<ETalentAcquired>(OnTalentAcquired);
            _eventBus.Subscribe<EEffectsApplied>(OnEffectsApplied);
            _eventBus.Subscribe<EBonusCollected>(OnBonusCollected);
            _eventBus.Subscribe<ECoinCollected>(OnCoinCollected);
            _eventBus.Subscribe<EPlayerFallInDeadBox>(OnDeadBox);
            _eventBus.Subscribe<EGameDataLoaded>(OnGameLoaded);
            _eventBus.Subscribe<ELevelUpUiCommand>(OnLevelUpUiCommand);
            _eventBus.Subscribe<EEnemyStompedEvent>(OnEnemyStompedEvent);
            _eventBus.Subscribe<EPitsGenerated>(OnPitsGenerated);
            _eventBus.Subscribe<EEnemyDied>(OnEnemyDied);
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnRestOnStatue);
            _eventBus.Subscribe<EItemPurchased>(OnItemPurchased);
            _eventBus.Subscribe<EPlayerEnteredCombatRoom>(OnPlayerEnteredCombatRoom);
            _eventBus.Subscribe<EPlayerExitedCombatRoom>(OnPlayerExitedCombatRoom);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EItemsEquipped>(OnItemsEquipped);
            _eventBus.Unsubscribe<EItemsUnequipped>(OnItemsUnequipped);
            _eventBus.Unsubscribe<EPlayerReceiveDamage>(OnPlayerReceiveDamage);
            _eventBus.Unsubscribe<EPlayerReceiveContactDamageByEnemy>(OnPlayerReceiveContactDamage);
            _eventBus.Unsubscribe<EEnemyBulletHitPlayer>(OnEnemyBulletHitPlayer);
            _eventBus.Unsubscribe<EPoisonEffectFinished>(OnStopPoisonEffect);
            _eventBus.Unsubscribe<EBurnEffectFinished>(OnStopBurntEffect);
            _eventBus.Unsubscribe<ETalentAcquired>(OnTalentAcquired);
            _eventBus.Unsubscribe<EEffectsApplied>(OnEffectsApplied);
            _eventBus.Unsubscribe<EBonusCollected>(OnBonusCollected);
            _eventBus.Unsubscribe<ECoinCollected>(OnCoinCollected);
            _eventBus.Unsubscribe<EPlayerFallInDeadBox>(OnDeadBox);
            _eventBus.Unsubscribe<EGameDataLoaded>(OnGameLoaded);
            _eventBus.Unsubscribe<ELevelUpUiCommand>(OnLevelUpUiCommand);
            _eventBus.Unsubscribe<EEnemyStompedEvent>(OnEnemyStompedEvent);
            _eventBus.Unsubscribe<EPitsGenerated>(OnPitsGenerated);
            _eventBus.Unsubscribe<EEnemyDied>(OnEnemyDied);
            _eventBus.Unsubscribe<EPlayerRestOnStatue>(OnRestOnStatue);
            _eventBus.Unsubscribe<EItemPurchased>(OnItemPurchased);
            _eventBus.Unsubscribe<EPlayerEnteredCombatRoom>(OnPlayerEnteredCombatRoom);
            _eventBus.Unsubscribe<EPlayerExitedCombatRoom>(OnPlayerExitedCombatRoom);
        }

        #region Event Handlers
        private void OnItemsEquipped(EItemsEquipped e)
        {
            var equippedItemsData = e.Items.ConvertAll(item => DataSource.Instance.GetEquipmentItem(item.item));
            
            // Handle item equipped event
            foreach (var item in equippedItemsData)
            {
                AdditionalCharacteristics += item.characteristicsModifier;
                AdditionalAttributes += item.attributesModifier;
                AdditionalTalents.AddRange(item.talentsModifier);                
            }
            
            // Update player characteristics, attributes and status based on the equipped item
            CalculateAttributes();
            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void OnItemsUnequipped(EItemsUnequipped e)
        {
            var equippedItemsData = e.Items.ConvertAll(item => DataSource.Instance.GetEquipmentItem(item.item));
            
            // Handle item equipped event
            foreach (var item in equippedItemsData)
            {
                AdditionalCharacteristics -= item.characteristicsModifier;
                AdditionalAttributes -= item.attributesModifier;

                foreach (var talent in item.talentsModifier)
                {
                    // find and remove only one instance of the talent
                    AdditionalTalents.Remove(talent);
                }   
            }
            
            // Update player characteristics, attributes and status based on the equipped item
            CalculateAttributes();
            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void HandlingDamage(DamageOutput damage)
        {
            // Check if player has Invincibility talent - if so, ignore all damage
            if (IsInvincible)
            {
                return;
            }
            
            if (damage.healthDamage > 0)
            {
                var damageWithDefense = damage.healthDamage - (int) AllAttributes.Defense;
                
                if (damageWithDefense <= 0)
                {
                    damageWithDefense = 1;
                }
                
                Status.currentHealth -= damageWithDefense;
            }

            if (damage.burntBuildUp > 0)
            {
                Status.currentBurnAmount += damage.burntBuildUp;
            }
            
            if (damage.frostBuildUp > 0)
            {
                Status.currentFrostAmount += damage.frostBuildUp;
            }
            
            if (damage.poisonBuildUp > 0)
            {
                Status.currentPoisonAmount += damage.poisonBuildUp;
            }
            
            if (damage.directLifeSteal > 0)
            {
                Status.currentLifes -= damage.directLifeSteal;
            }
            
            CalculateStatus();
            SendPlayerStatsUpdated();
            
            _eventBus.Publish(new EDamageReceived(damage));
        }
        
        private void OnPlayerReceiveDamage(EPlayerReceiveDamage e)
        {
            HandlingDamage(e.Damage);
        }

        private void OnPlayerReceiveContactDamage(EPlayerReceiveContactDamageByEnemy e)
        {
            HandlingDamage(e.Damage);
        }
        
        private void OnEnemyBulletHitPlayer(EEnemyBulletHitPlayer e)
        {
            HandlingDamage(e.Damage);
        }

        private void OnStopPoisonEffect(EPoisonEffectFinished e)
        {
            Status.isPoisoned = false;
            Status.currentPoisonAmount = 0;

            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void OnStopBurntEffect(EBurnEffectFinished e)
        {
            Status.isBurned = false;
            Status.currentBurnAmount = 0;
            
            // +50% of defense when burn effect ends
            Attributes.Defense = (int)(Attributes.Defense * 2f);
            
            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void OnTalentAcquired(ETalentAcquired e)
        {
            if (Talents.Contains(e.Talent))
            {
                Debug.LogWarning($"Player already has talent {e.Talent}, ignoring acquisition.");
                return;
            }
            
            Talents.Add(e.Talent);
                
            SendPlayerStatsUpdated();
        }
        
        private void OnEffectsApplied(EEffectsApplied e)
        {
            var effects = e.Effects;

            foreach (var effect in effects)
            {
                _effectCalculator.ApplyEffect(effect);
            }
            
            // Update player characteristics, attributes and status based on the equipped item
            CalculateAttributes();
            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void OnBonusCollected(EBonusCollected e)
        {
            // Handle bonus collected event
            var bonusData = e.BonusData;
            Status.points += bonusData.PointsAmount;

            SendPlayerStatsUpdated();
            
            _eventBus.Publish(new EPointsCollected(bonusData.PointsAmount));
        }
        
        private void OnCoinCollected(ECoinCollected e)
        {
            // Handle coin collected event
            Status.coins += 1;
            SendPlayerStatsUpdated();
        }
        
        private void OnDeadBox(EPlayerFallInDeadBox e)
        {
            Status.currentHealth = 0;

            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void OnGameLoaded(EGameDataLoaded e)
        {
            var gameData = e.GameData;
            
            Talents = gameData.talents;
            AdditionalTalents = gameData.additionalTalents;
            Characteristics = gameData.characteristics;
            AdditionalCharacteristics = gameData.additionalCharacteristics;
            Attributes = gameData.attributes;
            AdditionalAttributes = gameData.additionalAttributes;
            Status = gameData.status;
            EffectsOnDeath = gameData.effectsOnDeath;
            
            SendPlayerStatsUpdated();
        }

        private void OnLevelUpUiCommand(ELevelUpUiCommand e)
        {
            var newCharacteristics = e.NewCharacteristics;
            var pointsCost = e.PointsToUse;
            LevelUp(newCharacteristics, pointsCost);
        }

        private void OnEnemyStompedEvent(EEnemyStompedEvent e)
        {
            // When attacking an enemy with a jump, recover some energy
            // TODO: Evaluate the way which the energy recovered is calculated based, for example, on a jump attack combo (like super mario with points)
            // Also the recovery attributes needs to be taken into account here
            var energyToRecover = 1f;
            
            var oldEnergyAmount = Status.currentEnergy;
            Status.currentEnergy += energyToRecover;
            
            CalculateStatus();
            SendPlayerStatsUpdated();
            
            if (oldEnergyAmount < AllAttributes.MaxEnergy)
            {
                _eventBus.Publish(new EEnergyChanged(energyToRecover));    
            }
        }
        
        private void OnPitsGenerated(EPitsGenerated e)
        {
            var energyToConsume = _pitModel.GetEnergyCost();
            
            Status.currentEnergy -= energyToConsume;
            
            CalculateStatus();
            SendPlayerStatsUpdated();
            
            _eventBus.Publish(new EEnergyChanged(-energyToConsume));
        }
        
        private void OnEnemyDied(EEnemyDied e)
        {
            var points = e.Enemy.PointsDrop;
            
            Status.points += points;
            
            CalculateStatus();
            SendPlayerStatsUpdated();
            
            _eventBus.Publish(new EPointsCollected(points));
        }
        
        private void OnRestOnStatue(EPlayerRestOnStatue e)
        {
            Status.currentHealth = AllAttributes.MaxHealth;
            Status.currentEnergy = AllAttributes.MaxEnergy;
            // TODO: Remove status effects
            
            CalculateStatus();
            SendPlayerStatsUpdated();
            
            _eventBus.Publish(new EHpRestored((int) AllAttributes.MaxHealth));
            _eventBus.Publish(new EEnergyChanged(AllAttributes.MaxEnergy));
        }
        
        private void OnItemPurchased(EItemPurchased e)
        {
            var price = e.PurchasedItem.Price * e.PurchasedItem.Quantity;

            if (Status.coins < price)
            {
                Debug.LogWarning("[PlayerModel] Not enough coins to purchase item.");
                return;
            }

            Status.coins -= price;

            CalculateStatus();
            SendPlayerStatsUpdated();
        }
        
        private void OnPlayerEnteredCombatRoom(EPlayerEnteredCombatRoom e)
        {
            _inCombatRoom = e.RoomState == CombatRoomState.Active;
        }
        
        private void OnPlayerExitedCombatRoom(EPlayerExitedCombatRoom e)
        {
            _inCombatRoom = false;
        }
        
        #endregion

        #region Business Logic
        /// <summary>
        /// Calculate player attributes based on characteristics.
        /// This method uses helper methods to calculate attributes
        /// based on each characteristic.
        /// The final attributes are the sum of all contributions.
        /// </summary>
        private void CalculateAttributes()
        {
            Attributes =
                UCalculatePlayerAttributes.AttributesFromCharacteristics(AllCharacteristics, PlayerAttributes.InitialValue());
        }

        /// <summary>
        /// Calculate status and emit events if needed.
        /// </summary>
        private void CalculateStatus()
        {
            var attributes = Attributes + AdditionalAttributes;

            if (Status.currentLifes <= 0)
            {
                _eventBus.Publish(new EGameOver());
                return;
            }

            if (Status.currentHealth <= 0)
            {
                Status.currentLifes -= 1;
                Status.currentHealth = attributes.MaxHealth;

                // Decrease life dependant attributes
                // TODO: decide the correct values
                AdditionalAttributes.PoisonResistance -= 1;
                AdditionalAttributes.BurnResistance -= 1;
                AdditionalAttributes.FrostResistance -= 1;
                AdditionalAttributes.ThrowAttackBaseDamage -= 1;
                AdditionalAttributes.JumpAttackBaseDamage -= 1;
                AdditionalAttributes.Defense -= 1;

                foreach (var effect in EffectsOnDeath)
                {
                    _effectCalculator.ApplyEffect(effect);
                }

                attributes = Attributes + AdditionalAttributes;

                if (Status.currentLifes <= 0)
                {
                    _eventBus.Publish(new EGameOver());
                }
                
                _eventBus.Publish(new ELifeLost(1));
            }

            if (Status.currentHealth > attributes.MaxHealth)
            {
                Status.currentHealth = attributes.MaxHealth;
            } else if (Status.currentHealth < 0)
            {
                Status.currentHealth = 0;
            }

            if (Status.currentEnergy > attributes.MaxEnergy)
            {
                Status.currentEnergy = attributes.MaxEnergy;
            } else if (Status.currentEnergy < 0)
            {
                Status.currentEnergy = 0;
            }

            if (Status.currentLifes > attributes.MaxLifes)
            {
                Status.currentLifes = attributes.MaxLifes;
            } else if (Status.currentLifes < 0)
            {
                Status.currentLifes = 0;
            }

            if (Status.currentPoisonAmount > attributes.PoisonResistance)
            {
                Status.currentPoisonAmount = attributes.PoisonResistance;
            }
            else if (Status.currentPoisonAmount < 0)
            {
                Status.currentPoisonAmount = 0;
            }

            if (Status.currentBurnAmount > attributes.BurnResistance)
            {
                Status.currentBurnAmount = attributes.BurnResistance;
            } 
            else if (Status.currentBurnAmount < 0)
            {
                Status.currentBurnAmount = 0;
            }

            if (Status.currentFrostAmount > attributes.FrostResistance)
            {
                Status.currentFrostAmount = attributes.FrostResistance;
            } else if (Status.currentFrostAmount < 0)
            {
                Status.currentFrostAmount = 0;
            }
            
            if (Status.currentPoisonAmount >= attributes.PoisonResistance && !Status.isPoisoned)
            {
                Status.isPoisoned = true;
                
                _eventBus.Publish(new EPlayerPoisoned());
            }
            
            if (Status.currentBurnAmount >= attributes.BurnResistance && !Status.isBurned)
            {
                Status.isBurned = true;
                
                _eventBus.Publish(new EPlayerBurnt());
                
                // -50% of defense while burned
                Attributes.Defense = (int)(Attributes.Defense * 0.5f);
            }
            
            if (Status.currentFrostAmount >= attributes.FrostResistance && !Status.isFrostbitten)
            {
                Status.isFrostbitten = true;
            }
            
            if (AdditionalAttributes.MaxHealth < 0)
            {
                AdditionalAttributes.MaxHealth = 0;
            }
            
            if (AdditionalAttributes.MaxEnergy < 0)
            {
                AdditionalAttributes.MaxEnergy = 0;
            }
            
            if (AdditionalAttributes.MaxLifes < 0)
            {
                AdditionalAttributes.MaxLifes = 0;
            }

            if (AdditionalAttributes.JumpAttackBaseDamage < 0)
            {
                AdditionalAttributes.JumpAttackBaseDamage = 0;
            }
            
            if (AdditionalAttributes.ThrowAttackBaseDamage < 0)
            {
                AdditionalAttributes.ThrowAttackBaseDamage = 0;
            }
            
            if (AdditionalAttributes.Defense < 0)
            {
                AdditionalAttributes.Defense = 0;
            }
            
            if (AdditionalAttributes.PoisonResistance < 0)
            {
                AdditionalAttributes.PoisonResistance = 0;
            }
            
            if (AdditionalAttributes.BurnResistance < 0)
            {
                AdditionalAttributes.BurnResistance = 0;
            }
            
            if (AdditionalAttributes.FrostResistance < 0)
            {
                AdditionalAttributes.FrostResistance = 0;
            }
        }
        
        private void LevelUp(PlayerCharacteristics newCharacteristics, int pointsCost)
        {
            Characteristics = newCharacteristics;
            Status.points -= pointsCost;

            _eventBus.Publish(new ELevelUp(Characteristics));
            
            // Update player characteristics, attributes and status based on the equipped item
            CalculateAttributes();
            CalculateStatus();
            SendPlayerStatsUpdated();
        }

        private string CreateAttributeSummaryLabel(string title, string key)
        {
            var attributesValue = Functions.GetFieldValue<float>(Attributes, key);
            var additionalAttributesValue = Functions.GetFieldValue<float>(AdditionalAttributes, key);
            return additionalAttributesValue switch
            {
                > 0 => $"{title}: {attributesValue} (+{additionalAttributesValue})",
                < 0 => $"{title}: {attributesValue} (-{additionalAttributesValue})",
                _ => $"{title}: {attributesValue}"
            };
        }
        
        private string CreateCharacteristicSummaryLabel(string title, string key)
        {
            var characteristicValue = Functions.GetFieldValue<int>(Characteristics, key);
            var additionalCharacteristicValue = Functions.GetFieldValue<int>(AdditionalCharacteristics, key);
            return additionalCharacteristicValue switch
            {
                > 0 => $"{title}: {characteristicValue} (+{additionalCharacteristicValue})",
                < 0 => $"{title}: {characteristicValue} (-{additionalCharacteristicValue})",
                _ => $"{title}: {characteristicValue}"
            };
        }
        
        private string CreateStatusSummaryLabel(string title, string key)
        {
            var statusValue = Functions.GetFieldValue<float>(Status, key);
            return $"{title}: {statusValue}";
        }

        public List<string> GetOtherLabels()
        {
            var labels = new List<string>();
            var level = Characteristics.GetLevelPoints();
            var additionalLevel = AdditionalCharacteristics.GetLevelPoints();
            var additionalLevelString = additionalLevel > 0 ? $" ( +{additionalLevel})" : "";
            var levelLabel = $"Level: {level}{additionalLevelString}";
            labels.Add(levelLabel);

            labels.Add(CreateCharacteristicSummaryLabel("VIT", "Vitality"));
            labels.Add(CreateCharacteristicSummaryLabel("FOR", "Strength"));
            labels.Add(CreateCharacteristicSummaryLabel("AGI", "Agility"));
            labels.Add(CreateCharacteristicSummaryLabel("HUM", "Human"));
            labels.Add(CreateCharacteristicSummaryLabel("ALN", "Alien"));
            labels.Add(CreateCharacteristicSummaryLabel("GOD", "God"));
            labels.Add(CreateStatusSummaryLabel("Points", "points"));

            var nextLevelPoints = ULevelUp.GetNextLevelPoints(Characteristics);
            var nextLevelPointsLabel = $"Next: {nextLevelPoints}";
            labels.Add(nextLevelPointsLabel);
            labels.Add(CreateStatusSummaryLabel("Coins", "coins"));

            labels.Add(CreateStatusSummaryLabel("Bonuses", "bonuses"));
            labels.Add(CreateStatusSummaryLabel("SBonuses", "sBonuses"));
            labels.Add(CreateStatusSummaryLabel("Memories", "memories"));

            return labels;
        }

        public List<string> GetAttributeLabels()
        {
            var labels = new List<string>();
            labels.Add(CreateAttributeSummaryLabel("JAtt", "JumpAttackBaseDamage"));
            labels.Add(CreateAttributeSummaryLabel("PDef", "Defense"));
            labels.Add(CreateAttributeSummaryLabel("TAtt", "ThrowAttackBaseDamage"));
            labels.Add(CreateAttributeSummaryLabel("ResP", "PoisonResistance"));
            labels.Add(CreateAttributeSummaryLabel("ResB", "BurnResistance"));
            labels.Add(CreateAttributeSummaryLabel("ResF", "FrostResistance"));
            labels.Add(CreateAttributeSummaryLabel("MaxH", "MaxHealth"));
            labels.Add(CreateAttributeSummaryLabel("MaxE", "MaxEnergy"));
            labels.Add(CreateAttributeSummaryLabel("Drop", "DropRate"));
            labels.Add(CreateAttributeSummaryLabel("Crit", "CritRate"));
            labels.Add(CreateAttributeSummaryLabel("Grab", "Grabbing"));
            labels.Add(CreateAttributeSummaryLabel("Pits", "PitNumber"));
            labels.Add(CreateAttributeSummaryLabel("EssS", "EssenceSlots"));
            labels.Add(CreateAttributeSummaryLabel("AOra", "AlienOratory"));
            labels.Add(CreateAttributeSummaryLabel("HOra", "Oratory"));
            labels.Add(CreateAttributeSummaryLabel("DashD", "DashDuration"));
            labels.Add(CreateAttributeSummaryLabel("DashR", "Recovery"));

            return labels;
        }

        public bool InCombat()
        {
            return _inCombatRoom;
        }

        #endregion

        #region Utility
        private void SendPlayerStatsUpdated()
        {
            var allTalents = new List<TalentId>();
            allTalents.AddRange(Talents);
            if (AdditionalTalents != null)
            {
                allTalents.AddRange(AdditionalTalents);    
            }
            
            // Publish an event to notify other systems of the update
            _eventBus.Publish(new EPlayerStatsUpdated(
                Characteristics + AdditionalCharacteristics,
                Attributes + AdditionalAttributes,
                Status, allTalents));
        }
        #endregion

        #region Interface Implementation

        public bool CanRun => AllTalents.Contains(TalentId.Run);
        public bool CanThrow => AllTalents.Contains(TalentId.Throw);
        public bool CanJump => AllTalents.Contains(TalentId.Jump);
        public bool CanDash => AllTalents.Contains(TalentId.Dash);
        public bool CanCreatePits => AllTalents.Contains(TalentId.Pit);
        public bool CanSwim => AllTalents.Contains(TalentId.Swim);
        public bool CanTeleport => AllTalents.Contains(TalentId.Teleport);
        public bool CanPillEating => AllTalents.Contains(TalentId.PillEater);
        public bool CanFlyingDash => AllTalents.Contains(TalentId.FlyingDash);
        public bool CanSmash => AllTalents.Contains(TalentId.Smash);
        public bool CanDoubleJump => AllTalents.Contains(TalentId.DoubleJump);
        public bool CanWallJump => AllTalents.Contains(TalentId.WallJump);
        public bool CanHeadbutt => AllTalents.Contains(TalentId.Headbutt);
        public bool CanClimb => AllTalents.Contains(TalentId.Climber);
        public bool IsInvincible => AllTalents.Contains(TalentId.Invincibility);
        
        public bool CanGrab(int weights, int difficulty)
        {
            var hasGrab = AllTalents.Any(talent => talent.Equals(TalentId.Grab));
            var weightCheck = PickUpWeightCheck(weights);
            var agilityCheck = PickUpDifficultyCheck(difficulty);

            return hasGrab && weightCheck && agilityCheck;
        }

        public bool PickUpWeightCheck(int weight)
        {
            var strength = AllCharacteristics.Strength;

            // TODO: Review the weight thresholds and strength requirements
            return weight switch
            {
                <= 2 => strength >= 1,
                <= 5 => strength >= 2,
                <= 10 => strength >= 4,
                <= 15 => strength >= 6,
                > 15 => strength >= 8
            };
        }

        public bool PickUpDifficultyCheck(int difficulty)
        {
            var agility = AllCharacteristics.Agility;

            // TODO: Review the difficulty thresholds and agility requirements
            return difficulty switch
            {
                <= 2 => agility >= 1,
                <= 5 => agility >= 2,
                <= 10 => agility >= 3,
                <= 15 => agility >= 5,
                > 15 => agility >= 8
            };
        }

        public bool CanLevelUp(PlayerCharacteristics upgrade)
        {
            var nextLevelCost = ULevelUp.GetLevelUpCost(Characteristics, upgrade);
            return Status.points >= nextLevelCost;
        }
        
        public List<InventoryItemSlot> GetTalentsSlots()
        {
            return AllTalents.Select(talent => DataSource.Instance.GetTalentItem(talent)).Select(talentItem => new InventoryItemSlot(talentItem)).ToList();
        }

        public void Save(Data.Entities.Game.GameSessionData gameData)
        {
            gameData.characteristics = Characteristics;
            gameData.additionalCharacteristics = AdditionalCharacteristics;
            gameData.attributes = Attributes;
            gameData.additionalAttributes = AdditionalAttributes;
            gameData.status = Status;
            gameData.talents = Talents;
            gameData.additionalTalents = AdditionalTalents;
            gameData.effectsOnDeath = EffectsOnDeath;
        }
        
        public bool CanGeneratePit() {
            // Has the pit talent
            var hasPitTalent = CanCreatePits;
        
            // Has enough energy to generate a pit
            var hasEnoughEnergy = Status.currentEnergy >= _pitModel.GetEnergyCost();
            
            // Has at least one pit object to spawn
            var hasPitObjectsFromEssences = _pitModel.GetDesiredObject() != null;

            return hasPitTalent && hasEnoughEnergy && hasPitObjectsFromEssences;
        }
        
        public int GetNumberOfPitsCanBeGenerated()
        {
            return (int) AllAttributes.PitNumber;
        }
        
        public void SetPlayerPosition(Vector3 position)
        {
            _playerPosition = position;
        }
        
        public Vector3 GetPlayerPosition()
        {
            return _playerPosition;
        }
        
        #endregion
    }
    
    #region Events
    /// <summary>
    /// Game over event. Published when the player died and lives = 0.
    /// </summary>
    public struct EGameOver {}
    
    /// <summary>
    /// When the player reach 0 hp but still has lifes left.
    /// </summary>
    public struct ELifeLost
    {
        public int NumLifesLost;
        
        public ELifeLost(int numLifesLost)
        {
            NumLifesLost = numLifesLost;
        }
    }
    
    /// <summary>
    /// Notify that the player is poisoned.
    /// </summary>
    public struct EPlayerPoisoned
    {
        public readonly int Duration; // in seconds
        public readonly int DamagePerTick; // health damage per tick
        public readonly int TickInterval; // in seconds
        
        public EPlayerPoisoned(int duration = 60, int damagePerTick = 1, int tickInterval = 10)
        {
            Duration = duration;
            DamagePerTick = damagePerTick;
            TickInterval = tickInterval;
        }
    }
    
    /// <summary>
    /// Notify that the player is burnt.
    /// </summary>
    public struct EPlayerBurnt
    {
        public readonly int Duration; // in seconds
        
        public EPlayerBurnt(int duration = 30)
        {
            Duration = duration;
        }
    }
    
    /// <summary>
    /// Notify that the player is frostbitten.
    /// </summary>
    public struct EPlayerFrostbitten
    {
        public readonly int Duration; // in seconds
        
        public EPlayerFrostbitten(int duration = 30)
        {
            Duration = duration;
        }
    }
    
    /// <summary>
    /// Invoked when player stats are updated.
    /// This includes characteristics, attributes, status and talents.
    /// </summary>
    public struct EPlayerStatsUpdated
    {
        public readonly PlayerCharacteristics Characteristics;
        public readonly PlayerAttributes Attributes;
        public readonly PlayerStatus Status;
        public readonly List<TalentId> Talents;
        
        public EPlayerStatsUpdated(PlayerCharacteristics characteristics, PlayerAttributes attributes, PlayerStatus status, List<TalentId> talents)
        {
            Characteristics = characteristics;
            Attributes = attributes;
            Status = status;
            Talents = talents;
        }
    }
    
    /// <summary>
    /// Invoked when player receive any type of damage.
    /// </summary>
    public struct EPlayerReceiveDamage
    {
        public readonly DamageOutput Damage;
        
        public EPlayerReceiveDamage(DamageOutput damage)
        {
            Damage = damage;
        }
    }

    public struct EPlayerReceiveContactDamageByEnemy
    {
        public readonly DamageOutput Damage;
        public readonly Vector3 EnemyPosition;
        
        public EPlayerReceiveContactDamageByEnemy(DamageOutput damage, Vector3 enemyPosition)
        {
            Damage = damage;
            EnemyPosition = enemyPosition;
        }
    }
    

    /// <summary>
    /// Notify that the player is not poisoned anymore.
    /// </summary>
    public struct EPoisonEffectFinished
    {
    }

    /// <summary>
    /// Notify to stop the burn effect on the player.
    /// </summary>
    public struct EBurnEffectFinished
    {
    }
    
    /// <summary>
    /// Notify to stop the frostbite effect on the player.
    /// </summary>
    public struct EFrostbiteEffectFinished 
    {
    }
    
    /// <summary>
    /// Notify that the player acquired a new talent (permanently).
    /// </summary>
    public struct ETalentAcquired
    {
        public readonly TalentId Talent;
        
        public ETalentAcquired(TalentId talent)
        {
            Talent = talent;
        }
    }
    
    /// <summary>
    /// Notify that effects have been applied to the player.
    /// </summary>
    public struct EEffectsApplied
    {
        public readonly List<EffectData> Effects;
        
        public EEffectsApplied(List<EffectData> effects)
        {
            Effects = effects;
        }
    }
    
    public struct ELevelUp
    {
        public readonly PlayerCharacteristics CharacteristicsIncrease;
        
        public ELevelUp(PlayerCharacteristics characteristicsIncrease)
        {
            CharacteristicsIncrease = characteristicsIncrease;
        }
    }

    public struct EEnergyChanged
    {
        public readonly float EnergyChange;
        
        public EEnergyChanged(float energyChange)
        {
            EnergyChange = energyChange;
        }
    }
    
    public struct EPointsCollected
    {
        public readonly int PointsAmount;
        
        public EPointsCollected(int pointsAmount)
        {
            PointsAmount = pointsAmount;
        }
    }
    
    public struct EDamageReceived
    {
        public readonly DamageOutput Damage;
        
        public EDamageReceived(DamageOutput damage)
        {
            Damage = damage;
        }
    }
    
    public struct EHpRestored
    {
        public readonly int HpAmount;
        
        public EHpRestored(int hpAmount)
        {
            HpAmount = hpAmount;
        }
    }
    
    public struct ELifeGained
    {
        public readonly int LifesAmount;
        
        public ELifeGained(int lifesAmount)
        {
            LifesAmount = lifesAmount;
        }
    }

    #endregion
    
}