using UnityEngine;

namespace Utils
{
    static public class Constants {
        // MARK: - Layers
        static public readonly string GroundLayer = "Ground";
        static public readonly string WallLayer = "Wall";
        static public readonly string PlayerHitBoxLayer = "PlayerHitBox";
        static public readonly string EnemyLayer = "Enemy";
        static public readonly string GrabbableGroundLayer = "GrabbableGround";
        static public readonly string InteractableLayer = "Interactable";
        static public readonly string EnemyBulletLayer = "EnemyBullet";
        static public readonly int GroundLayerNumber = LayerMask.NameToLayer(GroundLayer);
        static public readonly int WallLayerNumber = LayerMask.NameToLayer(WallLayer);
        static public readonly int PlayerHitBoxLayerNumber = LayerMask.NameToLayer(PlayerHitBoxLayer);
        static public readonly int EnemyLayerNumber = LayerMask.NameToLayer(EnemyLayer);
        static public readonly int GrabbableGroundLayerNumber = LayerMask.NameToLayer(GrabbableGroundLayer);
        static public readonly int InteractableLayerNumber = LayerMask.NameToLayer(InteractableLayer);
        static public readonly int EnemyBulletLayerNumber = LayerMask.NameToLayer(EnemyBulletLayer);

        // MARK: - Tags
        static public readonly string PlayerTag = "Player";
        static public readonly string CanvasTag = "Canvas";
        static public readonly string PlayerFeetTag = "PlayerFeet";
        static public readonly string DeadBoxTag = "DeadBox";
        static public readonly string LadderTag = "Ladder";
        static public readonly string OneWayPlatformTag = "OneWayPlatform";
        static public readonly string TwoWayPlatformTag = "TwoWayPlatform";
        static public readonly string RespawnPointTag = "RespawnPoint";
        static public readonly string EnemyThrowWeakAreaTag = "EnemyThrowWeakArea";
        static public readonly string EnemyJumpWeakAreaTag = "EnemyJumpWeakArea";
        static public readonly string EnemyAttackAreaTag = "EnemyAttackArea";
        static public readonly string EnemyThrowAreaTag = "EnemyThrowArea";
        static public readonly string StatueTag = "Statue";
        static public readonly string GrabbableTag = "Grabbable";
        static public readonly string PitObjectTag = "PitObject";
        static public readonly string StandardHiddenWallTag = "StandardHiddenWall";

        // MARK: - Object Names
        static public string _playerObjectName = "Player";
        static public string _groundTileName = "Proto_Ground";
        static public string _onewayTileName = "Proto_OneWay";
  
        // MARK: - UI Elements Names - Player Status
        static public readonly string HealthLabelName = "Health";
        static public readonly string EnergyLabelName = "Energy";
        static public readonly string LifesLabelName = "Lifes";
        static public readonly string CoinsLabelName = "Coins";
        static public readonly string PointsLabelName = "Points";
        static public readonly string JumpAttackBaseDamageLabelName = "JumpAttack";
        static public readonly string ThrowAttackBaseDamageLabelName = "ThrowAttack";
        static public readonly string DefenseLabelName = "Defense";
        static public readonly string DropRateLabelName = "DropRate";
        static public readonly string CritRateLabelName = "CritRate";
        static public readonly string PitSlotsLabelName = "PitSlots";
  
        // MARK: - UI Elements Names - Inventory Game Menu
        static public readonly string InventoryListViewName = "ItemList";
        static public readonly string InventorySelectedItemImageName = "ItemImage";
        static public readonly string InventorySelectedItemDescriptionName = "ItemDescription";
        static public readonly string InventoryItemImage = "ItemImage";
        static public readonly string InventoryItemName = "ItemName";
        static public readonly string InventoryItemDescription = "ItemDescription";
        static public readonly string InventoryItemQuantity = "ItemQuantity";
        static public readonly string InventoryItemTypeIcon = "ItemTypeIcon";
  
        // MARK: - UI Elements Names - Status Game Menu
        static public readonly string SaveSlotsListVieName = "SaveSlotsListView";
    
        // MARK: - Respawn Positions - Test Room
        static public readonly Vector3 TestRoomRespawnPosition = new(0f, 10f, 0f);
        // MARK: - Respawn Positions - Level -1.X
        static public readonly Vector3 RswoRespawnPoint = new(-1462.5f, -277f, 0f);
        // MARK: - Respawn Positions - Level 1.X
        static public readonly Vector3 Level11ParentHouseRespawnPosition = new(76f,-37.5f,0);
        static public readonly Vector3 BeforeElevatorLevel11 = new(498f, -16.5f, 0f);
        static public readonly Vector3 BeforeCombatRoomLevel11 = new(205, -51.5f, 0);
        
        // MARK: - Scene Names
        static public readonly string Level11ParentHouse = "1.1_parent_house";
        
        static public readonly string TestRoom = "TestRoom";
        static public readonly string GameplayScene = "GamePlay";
        
        // MARK: - Ink variables names
        static public readonly string IVHasClownKey = "HasClownKey";
        static public readonly string IVAlienOratoryValue = "AlienOratoryValue";
        static public readonly string IVHumanOratoryValue = "HumanOratoryValue";
    }
}