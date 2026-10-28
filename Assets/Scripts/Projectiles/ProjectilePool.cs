using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Scripting.LifecycleManagement;

namespace ScalingShmup.Projectiles
{
    // One pool per projectile prefab. Shooters name the bullet prefab they fire and call ForPrefab,
    // which returns that prefab's pool: a pool placed in the scene if there is one, otherwise a new
    // one created on first use. Bullets are never instantiated per shot.
    // AutoStaticsCleanup empties the registry when Play mode starts, so pools from a previous
    // session aren't reused when domain reload is turned off.
    [AutoStaticsCleanup]
    public partial class ProjectilePool : MonoBehaviour
    {
        private static readonly Dictionary<Projectile, ProjectilePool> Pools = new Dictionary<Projectile, ProjectilePool>();

        [SerializeField] private Projectile _prefab;
        [SerializeField, Min(0)] private int _defaultCapacity = 64;
        [SerializeField, Min(1)] private int _maxSize = 1024;

        private ObjectPool<Projectile> _pool;

        public static ProjectilePool ForPrefab(GameObject prefab)
        {
            if (prefab == null || !prefab.TryGetComponent(out Projectile projectile))
            {
                return null;
            }

            if (Pools.TryGetValue(projectile, out ProjectilePool existing) && existing != null)
            {
                return existing;
            }

            var created = new GameObject($"{prefab.name} Pool").AddComponent<ProjectilePool>();
            created._prefab = projectile;
            created.Register();
            return created;
        }

        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            if (_prefab != null && Pools.TryGetValue(_prefab, out ProjectilePool registered) && registered == this)
            {
                Pools.Remove(_prefab);
            }
        }

        public Projectile Get()
        {
            if (_pool == null)
            {
                CreatePool();
            }

            return _pool.Get();
        }

        private void Register()
        {
            if (_prefab != null)
            {
                Pools[_prefab] = this;
            }
        }

        private void CreatePool()
        {
            _pool = new ObjectPool<Projectile>(
                CreateProjectile,
                projectile => projectile.gameObject.SetActive(true),
                projectile => projectile.gameObject.SetActive(false),
                // Unity clears pools when Play mode ends, after the scene has already destroyed them.
                projectile =>
                {
                    if (projectile != null)
                    {
                        Destroy(projectile.gameObject);
                    }
                },
                collectionCheck: false,
                _defaultCapacity,
                _maxSize);
        }

        private Projectile CreateProjectile()
        {
            Projectile projectile = Instantiate(_prefab, transform);
            projectile.SetReleaseAction(_pool.Release);
            return projectile;
        }
    }
}
