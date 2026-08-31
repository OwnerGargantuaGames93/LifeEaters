using System;
using System.Collections;
using System.Linq;
using Boundary.Camera;
using Boundary.GamePlay;
using Boundary.Commands;
using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using Boundary.UI.Components;
using Boundary.Utils;
using Cinemachine;
using Control.Pit;
using Control.Player;
using Data.Damage;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

namespace Boundary.Player
{
    public class PlayerController : MonoBehaviour
    {
        private IPlayerModel _player;
        private IPitModel _pitModel;
        private IEventBus _eventBus;
        
        #region Gravity
        [Header("Gravity")] private const float OriginalGravityScale = 7f;
        #endregion

        #region Movement
        [SerializeField] private float speed = 8f;
        [SerializeField] private float runSpeed = 12f;
        [SerializeField] private float acceleration = 1.4f;
        [SerializeField] private float deceleration = 3f;
        [SerializeField] private float movementAnimationStopThreshold = 0.15f;
        [SerializeField] private float dashingPower = 22f;
        [SerializeField] private float dashingTime = 0.18f;
        private PlayerMovementState _movementState = PlayerMovementState.Idle;
        #endregion

        #region Controls
        private float _horizontalMoveInput;
        private float _verticalMoveInput;
        private bool _runIsPressed;
        private bool _crouchIsPressed;
        private bool _crouchWasPressedThisFrame;
        private bool _jumpWasPressedThisFrame;
        private bool _jumpIsPressed;
        private bool _dashWasPressedThisFrame;
        private bool _grabThrow;
        private bool _release;
        #endregion

        #region Jumping
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float jumpHoldingTime = 0.5f;
        [SerializeField] private float jumpHangAccelerationMult = 1.2f;
        [SerializeField] private float jumpHangMaxSpeedMult = 1.2f;
        [SerializeField] public float coyoteTime = 0.3f;
        private float _coyoteTimeCounter = 0f;
        private float _jumpHoldingTimeCounter = 0f;
        private PlayerJumpState _jumpState = PlayerJumpState.ReadyToJump;
        private bool _jumpRequested;
        private bool _doubleJumpRequested;
        #endregion
        
        #region Wall Sliding
        [SerializeField] private float wallSlidingSpeed = 2f;
        private PlayerWallSlideState _wallSlideState = PlayerWallSlideState.NotSliding;
        #endregion
        
        #region TwoWay Platform 
        [SerializeField] private GameObject currentTwoWayPlatform;
        #endregion

        #region Climbing
        [SerializeField] private float climbingSpeed = 8f;
        [SerializeField] private float climbDownFromTwoWayFixedSpeed = -32f;
        private bool _onLadder = false;
        #endregion

        #region Crouching
        // Fraction of the standing collider's height used while crouched (shrinks from the top,
        // feet stay planted). Sprite is drawn at half its standing height when crouching.
        [SerializeField] private float crouchColliderHeightMultiplier = 0.5f;
        private Vector2 _standingColliderSize;
        private Vector2 _standingColliderOffset;
        private Vector2 _hitBoxColliderSize;
        private Vector2 _hitBoxColliderOffset;
        #endregion
        
        #region Falling Check
        [SerializeField] private bool isFalling = false;
        private float _prevYPosition = 0f;
        #endregion
        
        #region Knockback
        [SerializeField] public float knockBackHorizontalForce = 25f;
        [SerializeField] public float knockBackVerticalForce = 10f;
        [SerializeField] public float invincibilityFramesDurationAfterHit = 1.5f;
        #endregion

        #region Jump Attack
        [SerializeField] private float jumpAttackVerticalBounceForce = 20f;
        #endregion

        #region Respawn
        [SerializeField] private float respawnCooldownTime = 2f;
        private Vector3 _respawnPosition;
        #endregion

        #region Death Sequence
        [SerializeField] private float deathBounceForce = 12f;
        [SerializeField] private int deathSortingOrderBoost = 1000;
        [SerializeField] private float deathOffscreenViewportMargin = 0.15f;
        [SerializeField] private float delayAfterOffscreenBeforeNextStep = 0.5f;
        private SpriteRenderer[] _spriteRenderers;
        private int[] _originalSortingOrders;
        private bool _isDying;
        #endregion

        #region Pits
        [SerializeField] private LayerMask pitLayer;
        [SerializeField] private float groundPitCheckExtraHeight = 0.25f;
        private GameObject _pitWhichPlayerIsOver;
        #endregion

        #region Grabbing
        private GrabObjects _grabObjectManager;
        private GameObject _grabbedObject;        
        #endregion

        #region Script Components
        private TouchingDirections _touchingDirections;
        private Rigidbody2D _cPlayer;
        [SerializeField] private BoxCollider2D standingCollider;
        private CinemachineImpulseSource _impulseSource;
        [SerializeField] private VisualTip visualTipPrefab;
        [SerializeField] private CapsuleCollider2D hitBoxCollider;
        public Canvas Canvas { get; private set; }
        
        #endregion

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _player = GameContext.Instance.Player;
            _pitModel = GameContext.Instance.Pit;

            Canvas = GameObject.FindGameObjectWithTag(Constants.CanvasTag).GetComponent<Canvas>();

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<ELifeLost>(OnLifeLost);
            _eventBus.Subscribe<EGameOver>(OnGameOver);
            _eventBus.Subscribe<EEnemyStompedEvent>(OnEnemyStomped);
            _eventBus.Subscribe<EPlayerReceiveContactDamageByEnemy>(OnEnemyContactDamage);
            _eventBus.Subscribe<EEnemyBulletHitPlayer>(OnEnemyBulletHitPlayer);
            _eventBus.Subscribe<ETeleportPlayerToPosition>(OnTeleportPlayerToPosition);
            _eventBus.Subscribe<ETeleportToLastStatue>(OnTeleportPlayerToLastRespawnPoint);
            _eventBus.Subscribe<EHpRestored>(OnHpRestored);
            _eventBus.Subscribe<EEnergyChanged>(OnEnergyChanged);
            _eventBus.Subscribe<EDamageReceived>(OnDamageReceived);
            _eventBus.Subscribe<EPointsCollected>(OnPointsCollected);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<ELifeLost>(OnLifeLost);
            _eventBus.Unsubscribe<EGameOver>(OnGameOver);
            _eventBus.Unsubscribe<EEnemyStompedEvent>(OnEnemyStomped);
            _eventBus.Unsubscribe<EPlayerReceiveContactDamageByEnemy>(OnEnemyContactDamage);
            _eventBus.Unsubscribe<EEnemyBulletHitPlayer>(OnEnemyBulletHitPlayer);
            _eventBus.Unsubscribe<ETeleportPlayerToPosition>(OnTeleportPlayerToPosition);
            _eventBus.Unsubscribe<ETeleportToLastStatue>(OnTeleportPlayerToLastRespawnPoint);
            _eventBus.Unsubscribe<EHpRestored>(OnHpRestored);
            _eventBus.Unsubscribe<EEnergyChanged>(OnEnergyChanged);
            _eventBus.Unsubscribe<EDamageReceived>(OnDamageReceived);
            _eventBus.Unsubscribe<EPointsCollected>(OnPointsCollected);
        }

        private void Start()
        {
            _cPlayer = GetComponent<Rigidbody2D>();
            _grabObjectManager = GetComponent<GrabObjects>();
            _touchingDirections = GetComponent<TouchingDirections>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();

            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
            _originalSortingOrders = _spriteRenderers.Select(r => r.sortingOrder).ToArray();

            _standingColliderSize = standingCollider.size;
            _standingColliderOffset = standingCollider.offset;
            _hitBoxColliderSize = hitBoxCollider.size;
            _hitBoxColliderOffset = hitBoxCollider.offset;

            _prevYPosition = transform.position.y;
        }

        private void CaptureControls()
        {
            _runIsPressed = UserInput.instance.RunIsPressed();

            _crouchIsPressed = UserInput.instance.CrouchIsPressed();
            _crouchWasPressedThisFrame = UserInput.instance.CrouchWasPressedThisFrame();

            _jumpWasPressedThisFrame = UserInput.instance.JumpWasPressedThisFrame();
            _jumpIsPressed = UserInput.instance.JumpIsPressed();

            _dashWasPressedThisFrame = UserInput.instance.DashWasPressedThisFrame();

            _grabThrow = UserInput.instance.GrabThrowWasPressedThisFrame();
            _release = UserInput.instance.ReleaseObjectWasPressedThisFrame();

            _horizontalMoveInput = UserInput.instance.moveInput.x;
            _verticalMoveInput = UserInput.instance.moveInput.y;
        }

        private void Update()
        {
            if (_isDying) return;

            CaptureControls();

            HandleMovementState();
            HandleWallSlidingState();
            HandleJumpState();
            
            Crouch();
            GrabCheck();
            IsOnPitCheck();
            IsFallingCheck();

            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            GetObjectFromPit();

            // PrintJumpState();
            // PrintMovementState();

            SyncPlayerPosition();
        }

        private void FixedUpdate()
        {
            if (_isDying) return;

            Move();
            WallSlide();
            Jump();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Event Handlers

        private void OnLifeLost(ELifeLost e)
        {
            StartCoroutine(DeathSequence(respawnAfter: true));
        }

        private void OnGameOver(EGameOver e)
        {
            StartCoroutine(DeathSequence(respawnAfter: false));
        }

        private void OnTeleportPlayerToPosition(ETeleportPlayerToPosition e)
        { 
            _respawnPosition = e.TargetPosition;
            StartCoroutine(Respawn());
        }

        private void OnTeleportPlayerToLastRespawnPoint(ETeleportToLastStatue e)
        {
            StartCoroutine(Respawn());
        }
        
        
        private void OnHpRestored(EHpRestored e)
        {
            StartCoroutine(SpawnHpRestoredVisualTip(e.HpAmount));
        }
        
        private void OnEnergyChanged(EEnergyChanged e)
        {
            StartCoroutine(SpawnEnergyRestoredVisualTip(e.EnergyChange));
        }
        
        private void OnDamageReceived(EDamageReceived e)
        {
            StartCoroutine(SpawnDamageVisualTip(e.Damage));
        }

        private void OnPointsCollected(EPointsCollected e)
        {
            SpawnPointsCollectedVisualTip(e.PointsAmount);
        }

        #endregion

        #region Moving

        private void HandleMovementState()
        {
            var hasRunTalent = _player.CanRun;
            var hasDashTalent = _player.CanDash;
            
            if (_grabbedObject is not null && _release)
            {
                BlockPlayerMovement(0.3f);
            }
            
            switch (_movementState)
            {
                case PlayerMovementState.Freeze:
                case PlayerMovementState.Dashing:
                    return;
                case PlayerMovementState.Climbing:
                {
                    if (!_onLadder)
                    {
                       SetMovementState(PlayerMovementState.Idle);
                    }
                    return;
                }
                case PlayerMovementState.Crouching:
                {
                    if (!_crouchIsPressed || !_touchingDirections.IsGrounded || _grabbedObject is not null)
                    {
                        SetMovementState(PlayerMovementState.Idle);
                    }
                    return;
                }
                case PlayerMovementState.Idle:
                case PlayerMovementState.Walking:
                case PlayerMovementState.Running:
                {
                    if (_onLadder && Math.Abs(_verticalMoveInput) > 0.01f && _grabbedObject is null)
                    {
                        SetMovementState(PlayerMovementState.Climbing);
                        return;
                    }

                    // Crouch always stops the player instantly, even mid-run with the movement key
                    // still held - responsiveness beats letting the run finish decelerating first.
                    if (_crouchIsPressed && _touchingDirections.IsGrounded && _grabbedObject is null)
                    {
                        SetMovementState(PlayerMovementState.Crouching);
                        _cPlayer.linearVelocity = new Vector2(0f, _cPlayer.linearVelocity.y);
                        return;
                    }

                    if (Mathf.Abs(_horizontalMoveInput) > 0.01f)
                    {
                        if (_runIsPressed && hasRunTalent)
                        {
                            SetMovementState(PlayerMovementState.Running);
                        }
                        else
                        {
                            SetMovementState(PlayerMovementState.Walking);
                        }
                    }
                    else
                    {
                        SetMovementState(PlayerMovementState.Idle);
                    }
                    
                    if (hasDashTalent && _dashWasPressedThisFrame && _touchingDirections.IsGrounded && !_grabbedObject && _wallSlideState == PlayerWallSlideState.NotSliding)
                    {
                        SetMovementState(PlayerMovementState.Dashing);
                        StartCoroutine(Dashing());
                    }
                    return;
                }
                default:
                    Debug.LogError("[PlayerController] Unknown PlayerMovementState: " + _movementState);
                    return;
            }
        }

        private void Move()
        {
            TurnCheck();
            
            switch (_movementState)
            {
                case PlayerMovementState.Freeze:
                    _cPlayer.linearVelocity = Vector2.zero;
                    return;
                case PlayerMovementState.Dashing:
                    return;
                case PlayerMovementState.Crouching:
                    _cPlayer.linearVelocity = new Vector2(0f, _cPlayer.linearVelocity.y);
                    return;
            }

            if (_movementState is PlayerMovementState.Climbing)
            {
                _cPlayer.gravityScale = 0f;
                
                if (currentTwoWayPlatform != null && _verticalMoveInput < 0f)
                {
                    _cPlayer.linearVelocity = new Vector2(_cPlayer.linearVelocity.x, climbDownFromTwoWayFixedSpeed);
                }
                else
                {
                    _cPlayer.linearVelocity = new Vector2(_horizontalMoveInput * climbingSpeed, _verticalMoveInput * climbingSpeed);
                }
                
                return;
            } 
            
            _cPlayer.gravityScale = OriginalGravityScale;

            var targetSpeed = _horizontalMoveInput * (_movementState == PlayerMovementState.Running ? runSpeed : speed);
            if (_jumpState is PlayerJumpState.GoingUpWithJump or PlayerJumpState.GoingUpWithDoubleJump)
            {
                targetSpeed *= jumpHangMaxSpeedMult;
            }

            var speedDiff = targetSpeed - _cPlayer.linearVelocity.x;
            if (speedDiff == 0)
            {
                return;
            }

            var accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            if (_jumpState is PlayerJumpState.GoingUpWithJump or PlayerJumpState.GoingUpWithDoubleJump)
            {
                accelRate *= jumpHangAccelerationMult;
            }

            var movement = speedDiff * accelRate;
            _cPlayer.AddForce(movement * Vector2.right, ForceMode2D.Force);
        }
        
        private void SetMovementState(PlayerMovementState newState)
        {
            if (newState == PlayerMovementState.Crouching && _movementState != PlayerMovementState.Crouching)
            {
                ApplyCrouchingCollider();
            }
            else if (newState != PlayerMovementState.Crouching && _movementState == PlayerMovementState.Crouching)
            {
                RestoreStandingCollider();
            }

            _movementState = newState;
        }

        // Shrinks a collider's height by crouchColliderHeightMultiplier, keeping its bottom edge
        // fixed in place (the player ducks from the head down, feet stay planted).
        private static (Vector2 size, Vector2 offset) CrouchedColliderShape(Vector2 standingSize, Vector2 standingOffset, float heightMultiplier)
        {
            var crouchedHeight = standingSize.y * heightMultiplier;
            var offsetShift = (standingSize.y - crouchedHeight) * 0.5f;
            return (new Vector2(standingSize.x, crouchedHeight), new Vector2(standingOffset.x, standingOffset.y - offsetShift));
        }

        private void ApplyCrouchingCollider()
        {
            (standingCollider.size, standingCollider.offset) =
                CrouchedColliderShape(_standingColliderSize, _standingColliderOffset, crouchColliderHeightMultiplier);
            (hitBoxCollider.size, hitBoxCollider.offset) =
                CrouchedColliderShape(_hitBoxColliderSize, _hitBoxColliderOffset, crouchColliderHeightMultiplier);
        }

        private void RestoreStandingCollider()
        {
            standingCollider.size = _standingColliderSize;
            standingCollider.offset = _standingColliderOffset;
            hitBoxCollider.size = _hitBoxColliderSize;
            hitBoxCollider.offset = _hitBoxColliderOffset;
        }

        #endregion

        #region Wall Sliding
        
        private void HandleWallSlidingState()
        {
            var canWallSlideAndJump = _player.CanWallJump;

            if (_touchingDirections.IsOnWall && !_touchingDirections.IsGrounded && _horizontalMoveInput != 0f &&
                !_grabbedObject && canWallSlideAndJump)
            {
                SetWallSlideState(PlayerWallSlideState.Sliding);
            }
            else
            {
                SetWallSlideState(PlayerWallSlideState.NotSliding);
            }
        }

        private void WallSlide()
        {
            switch (_wallSlideState)
            {
                case PlayerWallSlideState.NotSliding:
                    return;
                case PlayerWallSlideState.Sliding:
                    _cPlayer.linearVelocity = new Vector2(_cPlayer.linearVelocity.x,
                        Mathf.Clamp(_cPlayer.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));
                    return;
                default:
                    Debug.LogError("[PlayerController] Unknown PlayerWallSlideState: " + _wallSlideState);
                    return;
            }
        }

        private void SetWallSlideState(PlayerWallSlideState newState)
        {
            _wallSlideState = newState;
        }

        #endregion

        #region Jumping

        private void HandleJumpState()
        {
            var jumpAvailable = _player.CanJump;
            var doubleJumpAvailable = _player.CanDoubleJump;
            
            // Stop here if movement is frozen, or ducking (no crouch-jump)
            if (_movementState is PlayerMovementState.Freeze or PlayerMovementState.Crouching)
            {
                return;
            }
            
            switch (_jumpState)
            {
                case PlayerJumpState.ReadyToJump:
                {
                    if (_jumpWasPressedThisFrame && jumpAvailable)
                    {
                        SetJumpState(PlayerJumpState.GoingUpWithJump);
                        _jumpHoldingTimeCounter = jumpHoldingTime;
                        _jumpRequested = true;
                        _coyoteTimeCounter = 0f;
                    }
                    else if (!_touchingDirections.IsGrounded && _wallSlideState is PlayerWallSlideState.NotSliding && _movementState is not PlayerMovementState.Climbing)
                    {
                        SetJumpState(PlayerJumpState.InsideCoyoteWindow);
                        _coyoteTimeCounter = coyoteTime;
                    }
                    return;
                }
                case PlayerJumpState.InsideCoyoteWindow:
                {
                    if (_jumpWasPressedThisFrame && jumpAvailable)
                    {
                        SetJumpState(PlayerJumpState.GoingUpWithJump);
                        _jumpHoldingTimeCounter = jumpHoldingTime;
                        _jumpRequested = true;
                        _coyoteTimeCounter = 0f;
                    }
                    else
                    {
                        if (_touchingDirections.IsGrounded)
                        {
                            SetJumpState(PlayerJumpState.ReadyToJump);
                            _coyoteTimeCounter = 0f;
                        }
                        else
                        {
                            _coyoteTimeCounter -= Time.deltaTime;
                            if (_coyoteTimeCounter <= 0f)
                            {
                                SetJumpState(PlayerJumpState.FallingWithoutJumping);
                            }   
                        }
                    }
                    return;
                }
                case PlayerJumpState.FallingWithoutJumping:
                {
                    if (_jumpWasPressedThisFrame && doubleJumpAvailable)
                    {
                        SetJumpState(PlayerJumpState.GoingUpWithDoubleJump);
                        _jumpHoldingTimeCounter = jumpHoldingTime;
                        _doubleJumpRequested = true;
                    }

                    if (_touchingDirections.IsGrounded)
                    {
                        SetJumpState(PlayerJumpState.ReadyToJump);
                    }
                    
                    return;
                }
                case PlayerJumpState.GoingUpWithJump:
                {
                    _jumpHoldingTimeCounter -= Time.deltaTime;

                    if (!_jumpIsPressed || _jumpHoldingTimeCounter <= 0f)
                    {
                        SetJumpState(PlayerJumpState.FallingAfterJump);
                    }
                    return;
                }
                case PlayerJumpState.FallingAfterJump:
                {
                    if (_jumpWasPressedThisFrame && doubleJumpAvailable)
                    {
                        SetJumpState(PlayerJumpState.GoingUpWithDoubleJump);
                        _jumpHoldingTimeCounter = jumpHoldingTime;
                        _doubleJumpRequested = true;
                    } else if (_touchingDirections.IsGrounded)
                    {
                        SetJumpState(PlayerJumpState.ReadyToJump);
                    }
                    return;
                }
                case PlayerJumpState.GoingUpWithDoubleJump:
                {
                    _jumpHoldingTimeCounter -= Time.deltaTime;

                    if (!_jumpIsPressed || _jumpHoldingTimeCounter <= 0f)
                    {
                        SetJumpState(PlayerJumpState.FallingAfterDoubleJump);
                    }
                    return;
                }
                case PlayerJumpState.FallingAfterDoubleJump:
                {
                    if (_touchingDirections.IsGrounded)
                    {
                        SetJumpState(PlayerJumpState.ReadyToJump);
                    }
                    return;
                }
                default:
                {
                    Debug.LogError("[PlayerController] Unknown PlayerJumpState: " + _jumpState);
                    return;
                }
            }
        }

        private void Jump()
        {
            if (_jumpRequested)
            {
                _cPlayer.AddForce(Vector2.up, ForceMode2D.Impulse);
                _jumpRequested = false;
            }

            if (_doubleJumpRequested)
            {
                _cPlayer.AddForce(Vector2.up, ForceMode2D.Impulse);
                _doubleJumpRequested = false;
            }
            
            if (_jumpState is PlayerJumpState.GoingUpWithJump or PlayerJumpState.GoingUpWithDoubleJump)
            {
                _cPlayer.linearVelocity = new Vector2(
                    _cPlayer.linearVelocity.x,
                    jumpForce
                );
            }
        }
        
        private void SetJumpState(PlayerJumpState newState)
        {
            _jumpState = newState;
        }

        #endregion
        
        #region Crouching

        private void Crouch()
        {
            if (!_crouchWasPressedThisFrame && !_crouchIsPressed)
            {
                return;
            }

            // If we are pressing crouch, we are on a one way platform, we need to fall down
            if (currentTwoWayPlatform)
            {
                StartCoroutine(DisableCollisionForTwoWayPlatform());
            }
        }

        #endregion

        #region Grabbing

        private void GetObjectFromPit()
        {
            if (_pitWhichPlayerIsOver && _grabThrow && !_grabbedObject)
            {
                var pitObject = _pitModel.GetDesiredObject();

                if (pitObject == null)
                {
                    Debug.Log("No object available in the pit");
                }
                else if (pitObject.@object && _player.CanGrab(pitObject.weight, pitObject.difficulty))
                {
                    _grabObjectManager.GrabObject(pitObject);
                }
                else
                {
                    // TODO: Add some type of feedback in order to notify the player that cannot pick up item due to low player characteristics
                    Debug.Log("Can't grabbing because of your low characteristics");
                }

                // Destroy pit which player is over
                Destroy(_pitWhichPlayerIsOver);
                _pitWhichPlayerIsOver = null;
            }
        }

        #endregion

        #region Jump Attack
        
        private void OnEnemyStomped(EEnemyStompedEvent e)
        {
            _cPlayer.linearVelocity = new Vector2(_cPlayer.linearVelocity.x, jumpAttackVerticalBounceForce);
        }

        #endregion

        #region Received Damage
        
        private void OnEnemyContactDamage(EPlayerReceiveContactDamageByEnemy e)
        {
            // Apply feedback to the player
            AttackReceivedFeedback(e.EnemyPosition);

            // Avoid collision with the enemies for a while
            StartCoroutine(InvincibilityFrames(invincibilityFramesDurationAfterHit));
        }
        
        private void OnEnemyBulletHitPlayer(EEnemyBulletHitPlayer e)
        {
            var direction = e.Direction; 
            
            // Apply feedback to the player
            AttackReceivedFeedback(direction);

            // Avoid collision with the enemies for a while
            StartCoroutine(InvincibilityFrames(invincibilityFramesDurationAfterHit));
        }
        
        private void AttackReceivedFeedback(Vector2 enemyPosition)
        {
            // Getting hit interrupts crouching (restores the standing collider) so the knockback
            // below isn't immediately zeroed out by Move()'s crouch handling.
            if (_movementState == PlayerMovementState.Crouching)
            {
                SetMovementState(PlayerMovementState.Idle);
            }

            CameraShake.instance.ExecCameraShake(_impulseSource);
            
            if (enemyPosition.x > transform.position.x)
            {
                _cPlayer.linearVelocity = new Vector2(-knockBackHorizontalForce, knockBackVerticalForce);
            }
            else
            {
                _cPlayer.linearVelocity = new Vector2(knockBackHorizontalForce, knockBackVerticalForce);
            }
        }

        // Avoid collision with the enemies for a period of time
        private IEnumerator InvincibilityFrames(float duration)
        {
            DisableEnemyDamage();
            yield return new WaitForSeconds(duration);
            EnableEnemyDamage();
        }

        private void DisableEnemyDamage()
        {
            _eventBus.Publish(new EPlayerInvincibilityChanged(true));
            Physics2D.IgnoreLayerCollision(Constants.PlayerHitBoxLayerNumber, Constants.EnemyLayerNumber, true);
            Physics2D.IgnoreLayerCollision(Constants.PlayerHitBoxLayerNumber, Constants.EnemyBulletLayerNumber, true);
        }
        
        private void EnableEnemyDamage()
        {
            _eventBus.Publish(new EPlayerInvincibilityChanged(false));
            Physics2D.IgnoreLayerCollision(Constants.PlayerHitBoxLayerNumber, Constants.EnemyLayerNumber, false);
            Physics2D.IgnoreLayerCollision(Constants.PlayerHitBoxLayerNumber, Constants.EnemyBulletLayerNumber, false);
        }

        #endregion

        #region Death Sequence

        private IEnumerator DeathSequence(bool respawnAfter)
        {
            if (_isDying) yield break;
            _isDying = true; // also gates Update()/FixedUpdate() so nothing fights the bounce/fall physics below

            DisableAllCollisions(true);
            _eventBus.Publish(new EPlayerDeathSequenceStarted());
            _eventBus.Publish(new EPlayerDied());

            _cPlayer.gravityScale = OriginalGravityScale;
            yield return StartCoroutine(DeathFallEffect.Play(
                transform, _cPlayer, _spriteRenderers,
                deathBounceForce, deathSortingOrderBoost, deathOffscreenViewportMargin));

            yield return new WaitForSeconds(delayAfterOffscreenBeforeNextStep);

            transform.rotation = Quaternion.identity;
            for (var i = 0; i < _spriteRenderers.Length; i++)
            {
                _spriteRenderers[i].sortingOrder = _originalSortingOrders[i];
            }

            DisableAllCollisions(false);
            SetMovementState(PlayerMovementState.Idle);
            _isDying = false;
            _eventBus.Publish(new EPlayerDeathSequenceEnded());

            if (respawnAfter)
            {
                yield return StartCoroutine(Respawn());
                _eventBus.Publish(new EPlayerRespawned());
            }
            else
            {
                _eventBus.Publish(new EGameOverSequenceFinished());
            }
        }

        private void DisableAllCollisions(bool disable)
        {
            foreach (var playerCollider in GetComponentsInChildren<Collider2D>())
            {
                playerCollider.enabled = !disable;
            }
        }

        #endregion

        #region Respawn

        // TODO: Improve respawning (animation, wait time, etc)
        private IEnumerator Respawn()
        {
            yield return null;
            // The player will traverse some respawn area. Those areas will set a respawn position
            // If no respawn position is set, the player will respawn in the nearest respawn point (very rare case)
            // Set velocity to zero before respawning in order to avoid carry momentum from previous position
            _cPlayer.linearVelocity = Vector2.zero;
            transform.position = _respawnPosition;
            StartCoroutine(RespawnCooldown());
        }

        private IEnumerator RespawnCooldown()
        {
            Physics2D.IgnoreLayerCollision(Constants.PlayerHitBoxLayerNumber, Constants.EnemyLayerNumber, true);
            // TODO: Start invincibility frames animation / effect
            yield return new WaitForSeconds(respawnCooldownTime);
            Physics2D.IgnoreLayerCollision(Constants.PlayerHitBoxLayerNumber, Constants.EnemyLayerNumber, false);
        }

        #endregion

        #region Checks
        // Check is the player is falling
        private void IsFallingCheck() {
            // Checking isFalling
            // Get current player y position
            var currentYPosition = transform.position.y;

            // If the current position is less than the previous position, the player is going down
            isFalling = currentYPosition < _prevYPosition;

            // Store the current position for the next frame
            _prevYPosition = currentYPosition;
        }

        private void TurnCheck()
        {
            transform.localScale = _horizontalMoveInput switch
            {
                > 0 => new Vector2(1, transform.localScale.y),
                < 0 => new Vector2(-1, transform.localScale.y),
                _ => transform.localScale
            };
        }
        
        private void IsOnPitCheck()
        {
            var startingPoint = new Vector3(
                standingCollider.bounds.center.x,
                standingCollider.bounds.min.y + groundPitCheckExtraHeight,
                standingCollider.bounds.center.z
            );

            var groundHit = Physics2D.BoxCast(startingPoint, standingCollider.bounds.size, 0f, Vector2.down, groundPitCheckExtraHeight, pitLayer);
            
            _pitWhichPlayerIsOver = groundHit.collider ? groundHit.collider.gameObject : null;
        }
        
        private void GrabCheck()
        {
            _grabbedObject = _grabObjectManager.GetCurrentGrabbedObject();
        }

        #endregion

        #region Triggers
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Constants.TwoWayPlatformTag))
            {
                currentTwoWayPlatform = collision.gameObject;
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Constants.TwoWayPlatformTag))
            {
                currentTwoWayPlatform = null;
            } 
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Constants.LadderTag) && _player.CanClimb)
            {
                _onLadder = true;
            } else if (collision.gameObject.CompareTag(Constants.RespawnPointTag) || 
                       collision.gameObject.CompareTag(Constants.StatueTag)) {
                _respawnPosition = collision.bounds.center;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Constants.LadderTag))
            {
                _onLadder = false; 
            }
        }
        #endregion

        #region Coroutines
        private IEnumerator Dashing()
        {
            _cPlayer.gravityScale = 0f;
            _cPlayer.linearVelocity = new Vector2(_cPlayer.transform.localScale.x * dashingPower, 0f);
            // TODO: start trail rendered
            DisableEnemyDamage();
            yield return new WaitForSeconds(dashingTime);
            // TODO: stop trail rendered
            _cPlayer.gravityScale = OriginalGravityScale;
            EnableEnemyDamage();
            SetMovementState(PlayerMovementState.Idle);
        }

        private IEnumerator DisableCollisionForTwoWayPlatform()
        {
            var tilemapCollider = currentTwoWayPlatform.GetComponent<TilemapCollider2D>();
            var compositeCollider = currentTwoWayPlatform.GetComponent<CompositeCollider2D>();

            Physics2D.IgnoreCollision(standingCollider, tilemapCollider);
            Physics2D.IgnoreCollision(standingCollider, compositeCollider);

            yield return new WaitForSeconds(0.25f);
        
            Physics2D.IgnoreCollision(standingCollider, compositeCollider, false);
            Physics2D.IgnoreCollision(standingCollider, tilemapCollider, false);
        }
        
        private IEnumerator RestorePlayerMovement(float durationInSeconds)
        {
            yield return new WaitForSeconds(durationInSeconds);
            SetMovementState(PlayerMovementState.Idle);
        }

        #endregion

        #region Animation Bridge Properties
        public bool IsWalking => _movementState == PlayerMovementState.Walking;
        public bool IsRunning => _movementState == PlayerMovementState.Running;
        public bool IsClimbing => _movementState == PlayerMovementState.Climbing;
        public bool IsDashing => _movementState == PlayerMovementState.Dashing;
        public bool IsClimbingMoving => IsClimbing && Mathf.Abs(_verticalMoveInput) > 0.01f;
        public bool IsWallSliding => _wallSlideState == PlayerWallSlideState.Sliding;

        // Based on actual vertical velocity (not the jump state machine), so it also covers
        // the enemy-stomp bounce (OnEnemyStomped), which never touches PlayerJumpState.
        public bool IsGoingUp => !_touchingDirections.IsGrounded && _cPlayer.linearVelocity.y > 0.01f;

        public bool IsCrouching => _movementState == PlayerMovementState.Crouching;

        // True while the rigidbody still has horizontal velocity, even after input is released
        // and the movement state already fell back to Idle - keeps the run animation playing
        // (slowing down via RunAnimSpeedMultiplier) through the deceleration instead of cutting to Idle.
        public bool IsPhysicallyMoving => Mathf.Abs(_cPlayer.linearVelocity.x) > movementAnimationStopThreshold;

        public float RunAnimSpeedMultiplier
        {
            get
            {
                var maxSpeed = IsRunning ? runSpeed : speed;
                return Mathf.Clamp01(Mathf.Abs(_cPlayer.linearVelocity.x) / maxSpeed);
            }
        }
        #endregion

        #region External Methods
        public static void MoveToPosition(Vector3 position)
        {
            var playerGameObject = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
            if (playerGameObject)
            {
                playerGameObject.transform.SetPositionAndRotation(position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("Player not found!");
            }
        }
        
        private void BlockPlayerMovement(float durationInSeconds)
        {
            _movementState = PlayerMovementState.Freeze;
            StartCoroutine(RestorePlayerMovement(durationInSeconds));
        }
        
        public bool IsFalling()
        {
            return isFalling;
        }
        #endregion

        #region Utilities
        private IEnumerator SpawnDamageVisualTip(DamageOutput damage)
        {
            yield return new WaitForSeconds(0.1f);
            
            var visualTip = SpawnVisualTip();
            visualTip.InitDamage(damage);
        }
        
        private IEnumerator SpawnHpRestoredVisualTip(float hpRestored)
        {
            // wait 0.3 seconds in order to avoid overlapping
            yield return new WaitForSeconds(0.1f);

            var visualTip = SpawnVisualTip();
            visualTip.InitHeal((int) hpRestored);
        }
        
        private IEnumerator SpawnEnergyRestoredVisualTip(float energyRestored)
        {
            // wait 0.3 seconds in order to avoid overlapping
            yield return new WaitForSeconds(0.3f);

            var visualTip = SpawnVisualTip();
            visualTip.InitEnergyGain((int) energyRestored);
        }
        
        private void SpawnPointsCollectedVisualTip(int points)
        {
            // wait 0.3 seconds in order to avoid overlapping
            var visualTip = SpawnVisualTip();
            visualTip.InitPointsGain(points);
        }

        private VisualTip SpawnVisualTip()
        {
            var worldPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            var visualTip = Instantiate(visualTipPrefab, Canvas.transform);
            
            // Set the initial world position - the visual tip will track this position in world space
            visualTip.SetInitialWorldPosition(worldPosition);


            return visualTip;
        }
        
        private void SyncPlayerPosition()
        {
            _player.SetPlayerPosition(transform.position);
        }
        
        #endregion

        #region Debug
        private void PrintJumpState()
        {
            const string prefix = "[Player Jump State] ";
            switch (_jumpState)
            {
                case PlayerJumpState.ReadyToJump:
                    Debug.Log(prefix + "ReadyToJump");
                    break;
                case PlayerJumpState.InsideCoyoteWindow:
                    Debug.Log(prefix + "Inside Coyote Window");
                    break;
                case PlayerJumpState.FallingWithoutJumping:
                    Debug.Log(prefix + "Falling Without Jumping");
                    break;
                case PlayerJumpState.GoingUpWithJump:
                    Debug.Log(prefix + "Going Up With Jump");
                    break;
                case PlayerJumpState.FallingAfterJump:
                    Debug.Log(prefix + "Falling After Jump");
                    break;
                case PlayerJumpState.GoingUpWithDoubleJump:
                    Debug.Log(prefix + "Going Up With Double Jump");
                    break;
                case PlayerJumpState.FallingAfterDoubleJump:
                    Debug.Log(prefix + "Falling After Double Jump");
                    break;
                default:
                    Debug.LogError(prefix + "Unknown PlayerJumpState: " + _jumpState);
                    break;
            }
        }
        
        private void PrintMovementState()
        {
            const string prefix = "[Player Movement State] ";
            switch (_movementState)
            {
                case PlayerMovementState.Idle:
                    Debug.Log(prefix + "Idle");
                    break;
                case PlayerMovementState.Walking:
                    Debug.Log(prefix + "Walking");
                    break;
                case PlayerMovementState.Running:
                    Debug.Log(prefix + "Running");
                    break;
                case PlayerMovementState.Freeze:
                    Debug.Log(prefix + "Freeze");
                    break;
                case PlayerMovementState.Dashing:
                    Debug.Log(prefix + "Dashing");
                    break;
                case PlayerMovementState.Climbing:
                    Debug.Log(prefix + "Climbing");
                    break;
                case PlayerMovementState.Crouching:
                    Debug.Log(prefix + "Crouching");
                    break;
                default:
                    Debug.LogError(prefix + "Unknown PlayerMovementState: " + _movementState);
                    break;
            }
        }
        #endregion
    }

    internal enum PlayerMovementState
    {
        Idle,
        Walking,
        Running,
        Freeze,
        Dashing,
        Climbing,
        Crouching,
        // Swim,
    }

    internal enum PlayerJumpState
    {
        ReadyToJump,
        InsideCoyoteWindow,
        FallingWithoutJumping,
        GoingUpWithJump,
        FallingAfterJump,
        GoingUpWithDoubleJump,
        FallingAfterDoubleJump,
        // IsSmashing,
        
    }
    
    internal enum PlayerWallSlideState
    {
        NotSliding,
        Sliding,
    }

    #region Events

    public struct ETeleportPlayerToPosition
    {
        public readonly Vector3 TargetPosition;

        public ETeleportPlayerToPosition(Vector3 position)
        {
            TargetPosition = position;
        }
    }
    
    public struct ETeleportToLastStatue {}

    public struct EGameOverSequenceFinished {}
    public struct EPlayerDeathSequenceStarted {}
    public struct EPlayerDeathSequenceEnded {}
    public struct EPlayerDied {}
    public struct EPlayerRespawned {}

    #endregion
}
