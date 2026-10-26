using UnityEngine;
using UnityEngine.Pool;

namespace ScalingShmup.Projectiles
{
    // One pool per projectile prefab. Shooters share a pool instead of instantiating bullets.
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile _prefab;
        [SerializeField, Min(0)] private int _defaultCapacity = 64;
        [SerializeField, Min(1)] private int _maxSize = 1024;

        private ObjectPool<Projectile> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<Projectile>(
                CreateProjectile,
                projectile => projectile.gameObject.SetActive(true),
                projectile => projectile.gameObject.SetActive(false),
                projectile => Destroy(projectile.gameObject),
                collectionCheck: false,
                _defaultCapacity,
                _maxSize);
        }

        public Projectile Get()
        {
            return _pool.Get();
        }

        private Projectile CreateProjectile()
        {
            Projectile projectile = Instantiate(_prefab, transform);
            projectile.SetReleaseAction(_pool.Release);
            return projectile;
        }
    }
}
