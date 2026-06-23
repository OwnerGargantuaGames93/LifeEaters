using Boundary.Camera;
using Boundary.Enemy.Behaviors.Chase.Shooting;
using Data.Damage;
using Data.Entities.Effects;
using Data.Entities.Projectile;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Projectile
{
    /// <summary>
    /// Runtime behaviour attached to every projectile prefab.
    /// Never Instantiate/Destroy this directly — always go through ProjectilePool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Projectile : MonoBehaviour
    {
        // ── Shader property IDs (cached for performance) ─────────────────────
        private static readonly int PropBaseColor    = Shader.PropertyToID("_BaseColor");
        private static readonly int PropSaturation   = Shader.PropertyToID("_Saturation");
        private static readonly int PropGlowRadius   = Shader.PropertyToID("_GlowRadius");
        private static readonly int PropGlowPulse    = Shader.PropertyToID("_GlowPulse");
        private static readonly int PropOutlineWidth = Shader.PropertyToID("_OutlineWidth");

        // ── References (set once in Awake) ───────────────────────────────────
        private Rigidbody2D      _rb;
        private SpriteRenderer   _sr;
        private CircleCollider2D _col;
        private MaterialPropertyBlock _propBlock;
        private IEventBus        _eventBus;

        // ── Runtime state ─────────────────────────────────────────────────────
        private ProjectileData   _data;
        private Vector2          _direction;
        private float            _lifetimeTimer;
        private bool             _isActive;

        // ── Invincibility tracking (shared across all projectile instances) ───
        private static bool _playerIsInvincible;

        // ── Colour map ────────────────────────────────────────────────────────
        private static readonly Color ColorWhite = Color.white;
        private static readonly Color ColorGreen = new(0.2f, 0.9f, 0.2f);
        private static readonly Color ColorRed   = new(0.95f, 0.2f, 0.1f);
        private static readonly Color ColorCyan  = new(0.2f, 0.85f, 1f);

        // ─────────────────────────────────────────────────────────────────────
        #region Unity Messages

        private void Awake()
        {
            _rb        = GetComponent<Rigidbody2D>();
            _sr        = GetComponent<SpriteRenderer>();
            _col       = GetComponent<CircleCollider2D>();
            _propBlock = new MaterialPropertyBlock();
            _eventBus  = GameContext.Instance.EventBus;

            _eventBus.Subscribe<EPlayerEnteredRoom>(OnPlayerEnteredRoom);
            _eventBus.Subscribe<EPlayerEnteredCombatRoom>(OnPlayerEnteredCombatRoom);
            _eventBus.Subscribe<EPlayerInvincibilityChanged>(OnPlayerInvincibilityChanged);
        }

        private void OnDestroy()
        {
            _eventBus.Unsubscribe<EPlayerEnteredRoom>(OnPlayerEnteredRoom);
            _eventBus.Unsubscribe<EPlayerEnteredCombatRoom>(OnPlayerEnteredCombatRoom);
            _eventBus.Unsubscribe<EPlayerInvincibilityChanged>(OnPlayerInvincibilityChanged);
        }

        private void Update()
        {
            if (!_isActive) return;

            // Lifetime countdown
            _lifetimeTimer += Time.deltaTime;
            if (_lifetimeTimer >= _data.Lifetime)
            {
                ReturnToPool();
                return;
            }

            // Ballistic movement (Physical relies on Rigidbody2D physics)
            if (_data.ProjectileType == ProjectileType.Ballistic)
                _rb.linearVelocity = _direction * _data.Speed;

            // Animate glow pulse (tier IV / V)
            if (_data.VisualConfig != null && _data.VisualConfig.GlowPulse)
            {
                _sr.GetPropertyBlock(_propBlock);
                float pulse = (Mathf.Sin(Time.time * 4f) * 0.5f + 0.5f); // 0–1
                _propBlock.SetFloat(PropGlowRadius, _data.VisualConfig.GlowRadius * (0.5f + pulse * 0.5f));
                _sr.SetPropertyBlock(_propBlock);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isActive) return;

            // Ballistic projectiles: only detect player via trigger
            if (other.CompareTag(Constants.PlayerTag))
            {
                HitPlayer();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!_isActive) return;

            // Physical projectiles use real collision
            if (other.collider.CompareTag(Constants.PlayerTag))
            {
                HitPlayer();
                return;
            }

            var isGround = other.gameObject.layer == Constants.GroundLayerNumber;
            var isWall   = other.gameObject.layer == Constants.WallLayerNumber;

            if (isGround || isWall)
            {
                if (_data.BounceFactor <= 0.01f)
                    ReturnToPool();
                // If BounceFactor > 0, the physics material on the Rigidbody2D handles the bounce
            }
        }

        private void HitPlayer()
        {
            // Respect invincibility frames — do NOT return to pool, let the projectile pass through
            if (_playerIsInvincible) return;

            _eventBus.Publish(new EEnemyBulletHitPlayer(_data.DamageOutput, transform.position));

            foreach (var effect in _data.OnHitEffects)
                _eventBus.Publish(new EProjectileEffectHitPlayer(effect));

            ReturnToPool();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Called by ProjectilePool when retrieving the projectile from the pool.
        /// </summary>
        public void Init(ProjectileData data, Vector2 spawnPosition, Vector2 direction)
        {
            _data          = data;
            _direction     = direction.normalized;
            _lifetimeTimer = 0f;
            _isActive      = true;

            transform.position = spawnPosition;

            // Physics setup — bodyType MUST be set before assigning velocity
            if (data.ProjectileType == ProjectileType.Ballistic)
            {
                _rb.bodyType       = RigidbodyType2D.Kinematic;
                _rb.gravityScale   = 0f;
                _rb.linearVelocity = _direction * data.Speed;
                // Ballistic: trigger only — detects player, passes through walls
                _col.isTrigger     = true;
            }
            else
            {
                _rb.bodyType        = RigidbodyType2D.Dynamic;
                _rb.gravityScale    = data.GravityScale;
                _rb.linearVelocity  = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.AddForce(_direction * data.Speed, ForceMode2D.Impulse);
                // Physical: non-trigger so Rigidbody2D collides with Ground/Wall tilemaps
                _col.isTrigger      = false;
            }

            // Visual
            ApplyVisual(data);

            // Face direction
            if (_direction.x != 0)
                transform.localScale = new Vector3(
                    Mathf.Sign(_direction.x) * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y,
                    transform.localScale.z);
        }

        /// <summary>
        /// Returns the projectile to the pool (via the pool's Release method).
        /// </summary>
        public void ReturnToPool()
        {
            if (!_isActive) return;
            _isActive = false;
            _rb.linearVelocity   = Vector2.zero;
            _rb.angularVelocity = 0f;
            ProjectilePool.Instance.Release(this);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Visual

        private void ApplyVisual(ProjectileData data)
        {
            _sr.GetPropertyBlock(_propBlock);

            // Base colour from effect type
            _propBlock.SetColor(PropBaseColor, GetEffectColor(data.EffectColor));

            // Saturation & glow from visual config
            if (data.VisualConfig != null)
            {
                _propBlock.SetFloat(PropSaturation,   data.VisualConfig.Saturation);
                _propBlock.SetFloat(PropGlowRadius,   data.VisualConfig.GlowRadius);
                _propBlock.SetFloat(PropGlowPulse,    data.VisualConfig.GlowPulse ? 1f : 0f);
            }
            else
            {
                _propBlock.SetFloat(PropSaturation,   1f);
                _propBlock.SetFloat(PropGlowRadius,   0f);
                _propBlock.SetFloat(PropGlowPulse,    0f);
            }

            // Outline — solid for Physical, none for Ballistic
            _propBlock.SetFloat(PropOutlineWidth,
                data.ProjectileType == ProjectileType.Physical ? 0.05f : 0f);

            _sr.SetPropertyBlock(_propBlock);
        }

        private static Color GetEffectColor(ProjectileEffectColor effectColor) =>
            effectColor switch
            {
                ProjectileEffectColor.Green => ColorGreen,
                ProjectileEffectColor.Red   => ColorRed,
                ProjectileEffectColor.Cyan  => ColorCyan,
                _                           => ColorWhite
            };

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Event Handlers

        private void OnPlayerEnteredRoom(EPlayerEnteredRoom e) => ReturnToPool();

        private void OnPlayerEnteredCombatRoom(EPlayerEnteredCombatRoom e) => ReturnToPool();

        private void OnPlayerInvincibilityChanged(EPlayerInvincibilityChanged e) =>
            _playerIsInvincible = e.IsInvincible;

        #endregion
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Published when a projectile carrying a status effect hits the player.</summary>
    public struct EProjectileEffectHitPlayer
    {
        public readonly EffectData Effect;

        public EProjectileEffectHitPlayer(EffectData effect)
        {
            Effect = effect;
        }
    }

    /// <summary>
    /// Published by PlayerController when invincibility frames start and end.
    /// All active projectiles will ignore the player while IsInvincible = true.
    /// </summary>
    public struct EPlayerInvincibilityChanged
    {
        public readonly bool IsInvincible;

        public EPlayerInvincibilityChanged(bool isInvincible)
        {
            IsInvincible = isInvincible;
        }
    }
    
    #region Events

    public struct EEnemyBulletHitPlayer
    {
        public readonly DamageOutput Damage;
        public readonly Vector3 Direction;
        
        public EEnemyBulletHitPlayer(DamageOutput damage, Vector3 direction)
        {
            Damage = damage;
            Direction = direction;
        }
    }
    
    #endregion
}

