using Boundary.GamePlay.Enemy.Base;
using Boundary.Utils;
using Data.Database;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Pit.Objects.Wheel
{
    public class WheelObject : MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [SerializeField]
        public PitObjectId PitObjectId;
  
        private PitObjectData Data;

        public DetectionZone hitBox;

        private Collider2D _collider;
  
        private Rigidbody2D _rigidbody;

        private TouchingDirections _touchingDirections;
  
        [SerializeField]
        private float throwForce = 50f;

        [SerializeField]
        private float wheelSpeed = 1.05f;
    
        private Vector2 wheelDirection;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            // Get the objects from a resource file using PitObjectPool
            Data = DataSource.Instance.GetPitObject(PitObjectId);
    
            _collider = GetComponent<Collider2D>();
    
            _rigidbody = GetComponent<Rigidbody2D>();
        
            _touchingDirections = GetComponent<TouchingDirections>();
        }

        private void Update() {
            if (hitBox.DetectedColliders.Count > 0)
            {
                if (hitBox.DetectedColliders.Exists(collider => collider.CompareTag(Constants.EnemyThrowWeakAreaTag)))
                {
                    // Get the enemy that is colliding with the object
                    var enemy = hitBox.DetectedColliders.Find(collider => collider.CompareTag(Constants.EnemyThrowWeakAreaTag)).GetComponentInParent<BaseEnemy>();
                    
                    _eventBus.Publish(new EEnemyHitByPitObjectEvent(enemy.Id, Data));

                    // For now, destroy the object
                    Destroy(gameObject);
                }
            }

            if (_touchingDirections.IsOnWall)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            // Check if the object is colliding with an enemy
            _rigidbody.AddForce(wheelSpeed * wheelDirection, ForceMode2D.Impulse);
        }

        public void Use(Vector2 direction)
        {
            wheelDirection = direction;
        
            _collider.enabled = true;
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            transform.SetParent(null);
        
            // For making wall touching direction work as expected
            transform.localScale = new Vector2(wheelDirection.x, transform.localScale.y);
        
            _rigidbody.AddForce(throwForce * wheelDirection, ForceMode2D.Impulse);
        }
    }
}
