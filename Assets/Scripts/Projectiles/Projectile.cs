using System;
using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
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
        private float _speed;
        private float _age;
        private bool _isLive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }

        public void SetReleaseAction(Action<Projectile> release)
        {
            _release = release;
        }

        // headingDegrees: 0 points along +X, 90 along +Y (up the screen).
        public void Launch(Vector2 position, float headingDegrees, ProjectileMotion motion, DamageInfo damage)
        {
            _motion = motion;
            _damage = damage;
            _pathPosition = position;
            _headingDegrees = headingDegrees;
            _speed = motion.Speed;
            _age = 0f;
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

            _speed = ClampSpeed(_speed + _motion.Acceleration * deltaTime);
            _headingDegrees += _motion.AngularVelocity * deltaTime;

            Vector2 heading = DirectionFromDegrees(_headingDegrees);
            _pathPosition += heading * (_speed * deltaTime);

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
                Despawn();
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

        private void Despawn()
        {
            _isLive = false;

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
