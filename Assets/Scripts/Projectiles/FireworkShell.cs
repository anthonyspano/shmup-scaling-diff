using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Projectiles
{
    // Add next to a Projectile. Once the shell has flown its range, it bursts like a firework:
    // it disappears and fires fragment bullets evenly around itself (8 by default, one along each
    // cardinal and intercardinal direction). It can also burst when it hits a target.
    [RequireComponent(typeof(Projectile))]
    public class FireworkShell : MonoBehaviour
    {
        [Tooltip("Distance in units the shell flies before it bursts.")]
        [SerializeField, Min(0.1f)] private float _range = 6f;
        [Tooltip("Also burst when the shell hits something before reaching its range.")]
        [SerializeField] private bool _burstOnHit = true;

        [Header("Burst")]
        [Tooltip("Bullet prefab fired in the burst. It needs a Projectile component, and must not be a firework itself.")]
        [SerializeField] private GameObject _fragmentPrefab;
        [Tooltip("8 gives one fragment per cardinal and intercardinal direction.")]
        [SerializeField, Min(1)] private int _fragmentCount = 8;
        [Tooltip("Degrees of the first fragment. 0 is right, 90 is up; the rest are spaced evenly around the circle.")]
        [SerializeField] private float _firstFragmentAngle = 0f;
        [SerializeField, Min(0f)] private float _fragmentDamage = 1f;
        [SerializeField] private ProjectileMotion _fragmentMotion = ProjectileMotion.Straight(10f);

        private Projectile _projectile;

        // Whoever fires the shell can change how far it flies, e.g. the Firework power-up.
        public float Range
        {
            get => _range;
            set => _range = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            _projectile = GetComponent<Projectile>();
        }

        private void OnEnable()
        {
            _projectile.Expired += OnExpired;
        }

        private void OnDisable()
        {
            _projectile.Expired -= OnExpired;
        }

        private void FixedUpdate()
        {
            if (_projectile.IsLive && _projectile.DistanceTravelled >= _range)
            {
                Burst();
                _projectile.Despawn();
            }
        }

        private void OnExpired(Projectile projectile, bool hitTarget)
        {
            if (hitTarget && _burstOnHit)
            {
                Burst();
            }
        }

        private void Burst()
        {
            ProjectilePool pool = ProjectilePool.ForPrefab(_fragmentPrefab);

            if (pool == null)
            {
                return;
            }

            // Fragments belong to whoever fired the shell, so they never hit that ship.
            DamageInfo shellDamage = _projectile.Damage;
            var damage = new DamageInfo(_fragmentDamage, shellDamage.Type, shellDamage.Source);
            Vector2 origin = transform.position;
            float step = 360f / _fragmentCount;

            for (int i = 0; i < _fragmentCount; i++)
            {
                pool.Get().Launch(origin, _firstFragmentAngle + i * step, _fragmentMotion, damage);
            }
        }
    }
}
