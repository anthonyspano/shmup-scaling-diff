using System.Collections;
using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.Patterns
{
    // Plays a sequence of BulletPatterns, firing projectiles from a shared pool.
    public class BulletPatternEmitter : MonoBehaviour
    {
        [Tooltip("Bullet prefab to fire. It needs a Projectile component.")]
        [SerializeField] private GameObject _bulletPrefab;
        [SerializeField] private BulletPattern[] _patterns;
        [SerializeField] private bool _loop = true;
        [SerializeField] private bool _playOnEnable = true;
        [Tooltip("Defaults to this transform.")]
        [SerializeField] private Transform _muzzle;
        [Tooltip("Used by patterns that aim at a target, usually the player.")]
        [SerializeField] private Transform _target;

        private Coroutine _routine;

        public bool IsPlaying => _routine != null;

        private void Awake()
        {
            if (_muzzle == null)
            {
                _muzzle = transform;
            }
        }

        private void OnEnable()
        {
            if (_playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            // Unity stops coroutines on disable, so only the handle needs clearing.
            _routine = null;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void Play()
        {
            Stop();

            if (_patterns != null && _patterns.Length > 0)
            {
                _routine = StartCoroutine(PlaySequence());
            }
        }

        public void Stop()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private IEnumerator PlaySequence()
        {
            bool playedAny;

            do
            {
                playedAny = false;

                foreach (BulletPattern pattern in _patterns)
                {
                    if (pattern != null)
                    {
                        playedAny = true;
                        yield return PlayPattern(pattern);
                    }
                }
            }
            // An array of empty slots would otherwise loop forever without yielding.
            while (_loop && playedAny);

            _routine = null;
        }

        private IEnumerator PlayPattern(BulletPattern pattern)
        {
            float startTime = Time.time;

            for (int burst = 0; burst < pattern.Bursts; burst++)
            {
                FireBurst(pattern, burst, Time.time - startTime);

                if (burst < pattern.Bursts - 1)
                {
                    yield return new WaitForSeconds(pattern.BurstInterval);
                }
            }

            yield return new WaitForSeconds(pattern.DelayAfter);
        }

        private void FireBurst(BulletPattern pattern, int burst, float time)
        {
            ProjectilePool pool = ProjectilePool.ForPrefab(_bulletPrefab);

            if (pool == null)
            {
                return;
            }

            Vector2 origin = _muzzle.position;
            float aimAngle = GetAimAngle(origin);
            ProjectileMotion motion = pattern.GetMotion(burst);
            var damage = new DamageInfo(pattern.Damage, DamageType.EnemyFire, gameObject);

            for (int bullet = 0; bullet < pattern.BulletsPerBurst; bullet++)
            {
                float heading = pattern.GetHeading(burst, bullet, time, aimAngle);
                pool.Get().Launch(origin, heading, motion, damage);
            }
        }

        // Without a target, aimed patterns fall back to firing straight down the screen.
        private float GetAimAngle(Vector2 origin)
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                return -90f;
            }

            Vector2 toTarget = (Vector2)_target.position - origin;
            return Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        }
    }
}
