using Data.Entities.Projectile;
using UnityEngine;
using UnityEngine.Pool;

namespace Boundary.GamePlay.Projectile
{
    /// <summary>
    /// Object pool for all enemy projectiles.
    /// Place this MonoBehaviour once in the persistent "GamePlay" scene.
    /// Enemies retrieve projectiles via ProjectilePool.Instance.Get(...)
    /// and projectiles return themselves via ProjectilePool.Instance.Release(...).
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }

        [Header("Prefab to pool — must have a Projectile component")]
        [SerializeField] private Projectile projectilePrefab;

        [Header("Pool settings")]
        [SerializeField] private int defaultCapacity  = 30;
        [SerializeField] private int maxPoolSize       = 200;

        private ObjectPool<Projectile> _pool;

        // ─────────────────────────────────────────────────────────────────────
        #region Unity Messages

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _pool = new ObjectPool<Projectile>(
                createFunc:      CreateProjectile,
                actionOnGet:     OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: false,          // disable in release builds for perf
                defaultCapacity: defaultCapacity,
                maxSize:         maxPoolSize
            );
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Public API

        /// <summary>
        /// Retrieves a projectile from the pool, initialises it and returns it.
        /// </summary>
        public Projectile Get(ProjectileData data, Vector2 spawnPosition, Vector2 direction)
        {
            Projectile p = _pool.Get();
            p.Init(data, spawnPosition, direction);
            return p;
        }

        /// <summary>
        /// Returns a projectile to the pool.
        /// Called internally by Projectile.ReturnToPool().
        /// </summary>
        public void Release(Projectile projectile)
        {
            _pool.Release(projectile);
        }

        /// <summary>
        /// Immediately returns ALL active projectiles to the pool.
        /// Call this when the player changes room.
        /// </summary>
        public void ReleaseAll()
        {
            // We collect active projectiles from the scene because the pool
            // only tracks inactive ones.
            Projectile[] active = FindObjectsByType<Projectile>(FindObjectsSortMode.None);
            foreach (Projectile p in active)
            {
                if (p.gameObject.activeSelf)
                    p.ReturnToPool();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Pool Callbacks

        private Projectile CreateProjectile()
        {
            Projectile p = Instantiate(projectilePrefab, transform);
            p.gameObject.SetActive(false);
            return p;
        }

        private static void OnGetFromPool(Projectile p)
        {
            p.gameObject.SetActive(true);
        }

        private static void OnReleaseToPool(Projectile p)
        {
            p.gameObject.SetActive(false);
        }

        private static void OnDestroyPoolObject(Projectile p)
        {
            if (p != null)
                Destroy(p.gameObject);
        }

        #endregion
    }
}

