using Boundary.Commands;
using Boundary.GamePlay.Projectile;
using Boundary.Pit.Objects.ClayBlock;
using Boundary.Pit.Objects.Wheel;
using Boundary.Pit.Objects.Wool;
using Control.Player;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Player
{
    public class GrabObjects : MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [SerializeField] private Transform grabCheck;
        [SerializeField] private float rayDistance = 4f;
        [SerializeField] private Transform grabPosition;
        [SerializeField] private LayerMask hitLayer;

        private PitObjectData _pitObject;
        private GameObject _grabbedObject;
        private const float ReleaseGestureForce = 100f;
        private const float PutDownForce = 30f;
        private bool _avoidImmediateReleasing;
        private Vector2 _playerDirection;

        // Update is called once per frame
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;

            _pitObject = null;
            _grabbedObject = null;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EPlayerReceiveContactDamageByEnemy>(OnPlayerReceiveContactDamage);
            _eventBus.Subscribe<EEnemyBulletHitPlayer>(OnPlayerReceiveBulletDamage);
        }

        private void Update()
        {
            _playerDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

            var grabThrowPressed = UserInput.instance.GrabThrowWasPressedThisFrame();
            var releasePressed = UserInput.instance.ReleaseObjectWasPressedThisFrame();
            
            // P Pressed
            if (_grabbedObject && grabThrowPressed)
            {
                // TODO: Find a better way
                if (_avoidImmediateReleasing)
                {
                    _avoidImmediateReleasing = false;
                } else
                {
                    // If the object is a pit object, use it
                    if (_pitObject != null)
                    {
                        UseObject();
                        _pitObject = null;
                    }
                    // otherwise throw it
                    else
                    {
                        ThrowObject();
                    }
                
                    _grabbedObject = null;
                }
            }
            // Pressed Down
            else if (_grabbedObject && releasePressed)
            {
                // if the object is a pit object, lose it 
                if (_pitObject != null)
                {
                    // Lose the object
                    // Noop for now
                }
                // otherwise put it down
                else
                {
                    PutDownObject();
                    _grabbedObject = null;
                }
            }
            else
            {
                // TODO: Evaluate to grab objects also objects under the player using ground check
                var hitInfo = Physics2D.Raycast(grabCheck.position, _playerDirection, rayDistance, hitLayer);

                // Grab Object
                if (hitInfo.collider)
                {
                    if (grabThrowPressed)
                    {
                        if (!_grabbedObject)
                        {
                            GrabObject(hitInfo.collider.gameObject, false);
                        }
                    }
                }
            }

            // Debug.DrawRay(grabCheck.position, _playerDirection * rayDistance, Color.magenta);
        }

        public GameObject GetCurrentGrabbedObject()
        {
            return _grabbedObject;
        }

        /**
        * Grab the object and position it on top of the head of the player
        */
        private void GrabObject(GameObject objectToGrab, bool air, bool isPitObject = false)
        {
            // Check if the object is grabbable
            if (!objectToGrab.CompareTag(Constants.GrabbableTag) && !isPitObject)
            {
                return;
            }
            
            _grabbedObject = objectToGrab;

            // Taken out from physics control
            _grabbedObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;

            // Position the object on top of the head of the player
            var objectYPosition = grabPosition.position.y + _grabbedObject.GetComponent<Collider2D>().bounds.extents.y;
            _grabbedObject.transform.position = new Vector3(grabPosition.position.x, objectYPosition, grabPosition.position.y);
            _grabbedObject.GetComponent<Collider2D>().enabled = false;
            _grabbedObject.transform.SetParent(transform);

            _avoidImmediateReleasing = air;
        }

        public void GrabObject(PitObjectData objectData)
        {
            _pitObject = objectData;
        
            var objectToInstantiate = _pitObject.@object;
            var instanceOfPitObject = Instantiate(objectToInstantiate);
        
            GrabObject(instanceOfPitObject, true, true);
        }

        /**
        * Throw the object with a force in the direction of the player
        */
        private void ThrowObject()
        {
            _grabbedObject.GetComponent<Collider2D>().enabled = true;
            _grabbedObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            _grabbedObject.transform.SetParent(null);
            _grabbedObject.GetComponent<Rigidbody2D>().AddForce(ReleaseGestureForce * _playerDirection, ForceMode2D.Impulse);
        }
    
        /**
        * Put down the previously grabbed object
        */
        private void PutDownObject()
        {
            _grabbedObject.GetComponent<Collider2D>().enabled = true;
            _grabbedObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            _grabbedObject.transform.SetParent(null);
            _grabbedObject.GetComponent<Rigidbody2D>().AddForce(PutDownForce * _playerDirection, ForceMode2D.Impulse);
        }

        /**
        * Use the grabbed pit object
        */
        private void UseObject()
        {
            switch (_pitObject.id)
            {
                case PitObjectId.ClayBlock:
                {
                    _grabbedObject.GetComponent<ClayBlockObject>().Use(_playerDirection);
                    break;
                }
                case PitObjectId.Wheel:
                {
                    _grabbedObject.GetComponent<WheelObject>().Use(_playerDirection);
                    break;  
                }
                case PitObjectId.Wool:
                {
                    _grabbedObject.GetComponent<WoolObject>().Use();
                    break;  
                }
                default:
                {
                    Debug.LogError($"Pit object {_pitObject.id} not implemented");
                    break;
                }
            }
        }
        
        private void LooseObject()
        {
            // ReSharper disable once InvertIf
            if (_pitObject != null)
            {
                Destroy(_grabbedObject);
                _pitObject = null;
                _grabbedObject = null;
            }
            else if (_grabbedObject != null)
            {
                PutDownObject();
            }
        }

        #region Event Handlers

        private void OnPlayerReceiveContactDamage(EPlayerReceiveContactDamageByEnemy e)
        {
            LooseObject();
        }
        
        private void OnPlayerReceiveBulletDamage(EEnemyBulletHitPlayer e)
        {
            LooseObject();
        }
        

        #endregion
    }
}
