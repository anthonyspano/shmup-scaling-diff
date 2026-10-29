using System;
using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Bullet speed in units per second. Overrides the speed set by whatever fires it; 0 keeps the shooter's speed.")]
        [SerializeField, Min(0f)] private float _speed = 0f;
        [SerializeField, Min(0.1f)] private float _lifetime = 5f;
        [SerializeField] private LayerMask _targetLayers;
        [Tooltip("Rotate the sprite to face its direction of travel.")]
        [SerializeField] private bool _alignToHeading = true;

        private Rigidbody2D _rigidbody;
        private ProjectileMotion _motion;
        private DamageInfo _damage;
        private Action<Projectile> _release;

        private Vector2 _pathPosition;
        private float _headingDegrees;
        private float _currentSpeed;
        private float _age;
        private float _distanceTravelled;
        private bool _isLive;

        // Raised as the projectile is removed. The bool is true when it was removed by hitting a target.
        public event Action<Projectile, bool> Expired;

        public DamageInfo Damage => _damage;
        public float HeadingDegrees => _headingDegrees;
        public float DistanceTravelled => _distanceTravelled;
        public bool IsLive => _isLive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;

            // Hits are detected with OnTriggerEnter2D. A kinematic body with a solid collider gets
            // no contacts at all against other kinematic bodies (like the boss), so force triggers.
            foreach (Collider2D projectileCollider in GetComponents<Collider2D>())
            {
                projectileCollider.isTrigger = true;
            }
        }

        public void SetReleaseAction(Action<Projectile> release)
        {
            _release = release;
        }

        // headingDegrees: 0 points along +X, 90 along +Y (up the screen).
        public float Speed => _speed;

        public void Launch(Vector2 position, float headingDegrees, ProjectileMotion motion, DamageInfo damage)
        {
            if (_speed > 0f)
            {
                // The shooter's curve, wave and acceleration still apply; only the base speed changes.
                motion.Speed = _speed;
                motion.SpeedRange.y = Mathf.Max(motion.SpeedRange.y, _speed);
            }

            _motion = motion;
            _damage = damage;
            _pathPosition = position;
            _headingDegrees = headingDegrees;
            _currentSpeed = motion.Speed;
            _age = 0f;
            _distanceTravelled = 0f;
            _isLive = true;

            transform.position = position;
            _rigidbody.position = position;
            ApplyRotation();
        }

        private void FixedUpdate()
        {
            if (!_isLive)
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            _age += deltaTime;

            if (_age >= _lifetime)
            {
                Despawn();
                return;
            }

            _currentSpeed = ClampSpeed(_currentSpeed + _motion.Acceleration * deltaTime);
            _headingDegrees += _motion.AngularVelocity * deltaTime;

            Vector2 heading = DirectionFromDegrees(_headingDegrees);
            _pathPosition += heading * (_currentSpeed * deltaTime);
            _distanceTravelled += _currentSpeed * deltaTime;

            // The wave is an offset from the path, so it never changes the bullet's overall course.
            Vector2 side = new Vector2(-heading.y, heading.x);
            float wave = _motion.WaveAmplitude * Mathf.Sin(2f * Mathf.PI * _motion.WaveFrequency * _age);

            _rigidbody.MovePosition(_pathPosition + side * wave);
            ApplyRotation();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLive || (_targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            // The shooter can share a layer with its targets, so never hit the ship that fired.
            if (_damage.Source != null && other.transform.IsChildOf(_damage.Source.transform))
            {
                return;
            }

            IDamageable target = other.GetComponentInParent<IDamageable>();

            if (target != null && target.IsAlive)
            {
                target.TakeDamage(_damage);
                Despawn(hitTarget: true);
            }
        }

        public static Vector2 DirectionFromDegrees(float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private float ClampSpeed(float speed)
        {
            Vector2 range = _motion.SpeedRange;
            return range.y > range.x ? Mathf.Clamp(speed, range.x, range.y) : Mathf.Max(0f, speed);
        }

        private void ApplyRotation()
        {
            if (_alignToHeading)
            {
                // Sprites are authored pointing up, so subtract 90 degrees from the heading.
                _rigidbody.rotation = _headingDegrees - 90f;
            }
        }

        // Removes the projectile now, e.g. when a firework shell reaches its range and bursts.
        public void Despawn(bool hitTarget = false)
        {
            if (!_isLive)
            {
                return;
            }

            _isLive = false;
            Expired?.Invoke(this, hitTarget);

            if (_release != null)
            {
                _release(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
