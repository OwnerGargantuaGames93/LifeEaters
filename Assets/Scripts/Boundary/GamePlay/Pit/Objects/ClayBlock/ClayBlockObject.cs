using Boundary.GamePlay.Enemy.Base;
using Boundary.Utils;
using Control.Inventory;
using Data.Database;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Boundary.Pit.Objects.ClayBlock
{
  public class ClayBlockObject: MonoBehaviour {
    private const float ThrowForce = 100f;
    
    private IEventBus _eventBus;
    private IInventoryModel _inventory;
  
    [FormerlySerializedAs("PitObjectId")]
    [SerializeField] public PitObjectId pitObjectId;  
  
    private PitObjectData _data;

    public DetectionZone hitBox;

    private Collider2D _collider;
  
    private Rigidbody2D _rigidbody;
    
    private bool _isUsed;

    private void Awake() {
      _eventBus = GameContext.Instance.EventBus;
      _inventory = GameContext.Instance.Inventory;
      
      _data = DataSource.Instance.GetPitObject(pitObjectId);
    
      _collider = GetComponent<Collider2D>();
    
      _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
      if (!_isUsed)
      {
        return;
      }
      
      if (hitBox.DetectedColliders.Count <= 0)
      {
        return;
      }

      if (!hitBox.DetectedColliders.Exists(coll => coll.CompareTag(Constants.EnemyThrowWeakAreaTag)))
      {
        return;
      }
      
      var enemyWeakAreaCollider = hitBox.DetectedColliders.Find(coll => coll.CompareTag(Constants.EnemyThrowWeakAreaTag) && coll.enabled);
      var enemy = enemyWeakAreaCollider.GetComponentInParent<BaseEnemy>();

      _eventBus.Publish(new EEnemyHitByPitObjectEvent(enemy.Id, _data));
      
      Destroy(gameObject);
    }

    public void Use(Vector2 direction)
    {
      _isUsed = true;
      _collider.enabled = true;
      _rigidbody.bodyType = RigidbodyType2D.Dynamic;
      transform.SetParent(null);
      _rigidbody.AddForce(ThrowForce * direction, ForceMode2D.Impulse);
    }

  }
}