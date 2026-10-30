using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.Enemies.Bosses
{
    // Sprays bullets while sweeping its aim back and forth between the screen's top-left and
    // bottom-left corners. The boss decides which corner each turret starts from. While given a
    // target (see BossTurretTargeting), it aims straight at the target at its own fire rate instead.
    public class BossTurret : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float _shotsPerSecond = 6f;
        [Tooltip("Fire rate while this turret is aiming at the player instead of sweeping.")]
        [SerializeField, Min(0.1f)] private float _aimedShotsPerSecond = 3f;
        [Tooltip("Seconds to sweep from one corner to the other.")]
        [SerializeField, Min(0.1f)] private float _sweepDuration = 1.5f;
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField] private ProjectileMotion _motion = ProjectileMotion.Straight(6f);
        [Tooltip("Bullet prefab to fire. It needs a Projectile component.")]
        [SerializeField] private GameObject _bulletPrefab;
        [Tooltip("Each shot plays this enemy's Shoot Sound (usually the Enemy prefab), so turrets sound like the enemy ships.")]
        [SerializeField] private Enemy _shootSoundFrom;

        private Enemy _owner;
        private Camera _camera;
        private bool _startsAtTop;
        private float _sweepTime;
        private float _nextShotTime;

        // Set to aim at something (e.g. the player); null to go back to sweeping.
        public Transform Target { get; set; }

        public void Configure(Enemy owner, Camera viewCamera, bool startsAtTop)
        {
            _owner = owner;
            _camera = viewCamera;
            _startsAtTop = startsAtTop;
            _sweepTime = 0f;
        }

        private void Update()
        {
            if (_camera == null)
            {
                return;
            }

            _sweepTime += Time.deltaTime;

            if (Time.time < _nextShotTime || (_owner != null && !_owner.CanShoot))
            {
                return;
            }

            ProjectilePool pool = ProjectilePool.ForPrefab(_bulletPrefab);

            if (pool == null)
            {
                return;
            }

            bool aiming = Target != null;
            _nextShotTime = Time.time + 1f / (aiming ? _aimedShotsPerSecond : _shotsPerSecond);
            float heading = aiming ? HeadingTo(Target.position) : CurrentHeading();
            var damage = new DamageInfo(_damage, DamageType.EnemyFire, _owner != null ? _owner.gameObject : gameObject);
            pool.Get().Launch(transform.position, heading, _motion, damage);

            if (_shootSoundFrom != null)
            {
                _shootSoundFrom.PlayShootSound();
            }
        }

        // 0 at the starting corner, 1 at the other, then back again.
        private float CurrentHeading()
        {
            float sweep = Mathf.PingPong(_sweepTime / _sweepDuration, 1f);
            float toTopLeft = HeadingTo(Corner(true));
            float toBottomLeft = HeadingTo(Corner(false));
            return _startsAtTop
                ? Mathf.LerpAngle(toTopLeft, toBottomLeft, sweep)
                : Mathf.LerpAngle(toBottomLeft, toTopLeft, sweep);
        }

        private Vector2 Corner(bool top)
        {
            Vector2 centre = _camera.transform.position;
            float halfHeight = _camera.orthographicSize;
            return new Vector2(centre.x - halfHeight * _camera.aspect, centre.y + (top ? halfHeight : -halfHeight));
        }

        private float HeadingTo(Vector2 point)
        {
            Vector2 direction = point - (Vector2)transform.position;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
    }
}
