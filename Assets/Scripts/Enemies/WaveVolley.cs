using System.Collections.Generic;
using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.Enemies
{
    // Wave shooting: on each run across the screen, every enemy fires once, the moment its sprite is
    // fully inside the camera's view. The first enemy of the run to get fully on screen aims at the
    // player, and the rest of the line fire along that same direction, so the shots are parallel.
    public class WaveVolley : MonoBehaviour
    {
        [Tooltip("Bullet prefab to fire. It needs a Projectile component.")]
        [SerializeField] private GameObject _bulletPrefab;
        [Tooltip("Usually the player. When it is missing, enemies hold fire.")]
        [SerializeField] private Transform _target;
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField] private ProjectileMotion _motion = ProjectileMotion.Straight(6f);
        [Tooltip("Defaults to Camera.main. Enemies fire once their sprite is fully inside its view.")]
        [SerializeField] private Camera _camera;

        // Per wave: who has already fired this run, and the run's shared heading once it is set.
        private readonly Dictionary<IReadOnlyList<Enemy>, RunState> _runs = new Dictionary<IReadOnlyList<Enemy>, RunState>();

        private class RunState
        {
            public readonly HashSet<Enemy> Fired = new HashSet<Enemy>();
            public float? Heading;
        }

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        // Call when a wave starts a new run, so every enemy gets to fire again.
        public void StartRun(IReadOnlyList<Enemy> wave)
        {
            _runs[wave] = new RunState();
        }

        public void EndWave(IReadOnlyList<Enemy> wave)
        {
            _runs.Remove(wave);
        }

        // Call every frame while the wave is alive. Fires for each enemy that has just come fully on screen.
        public void UpdateWave(IReadOnlyList<Enemy> wave)
        {
            if (!_runs.TryGetValue(wave, out RunState run))
            {
                run = new RunState();
                _runs[wave] = run;
            }

            bool hasTarget = _target != null && _target.gameObject.activeInHierarchy;

            foreach (Enemy enemy in wave)
            {
                if (run.Fired.Contains(enemy) || !IsFullyOnScreen(enemy))
                {
                    continue;
                }

                if (!run.Heading.HasValue)
                {
                    // Without a target, wait rather than fire in an arbitrary direction.
                    if (!hasTarget)
                    {
                        return;
                    }

                    Vector2 aim = (Vector2)_target.position - (Vector2)enemy.transform.position;
                    run.Heading = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                }

                run.Fired.Add(enemy);

                // An enemy that can't shoot still counts as having had its turn (and can set the aim).
                if (enemy.CanShoot)
                {
                    Fire(enemy, run.Heading.Value);
                }
            }
        }

        private void Fire(Enemy enemy, float heading)
        {
            ProjectilePool pool = ProjectilePool.ForPrefab(_bulletPrefab);

            if (pool != null)
            {
                var damage = new DamageInfo(_damage, DamageType.EnemyFire, enemy.gameObject);
                pool.Get().Launch(enemy.transform.position, heading, _motion, damage);
            }
        }

        private bool IsFullyOnScreen(Enemy enemy)
        {
            if (_camera == null || !_camera.orthographic)
            {
                return true;
            }

            Bounds bounds = enemy.TryGetComponent(out SpriteRenderer sprite)
                ? sprite.bounds
                : new Bounds(enemy.transform.position, Vector3.zero);

            Vector2 centre = _camera.transform.position;
            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            return bounds.min.x >= centre.x - halfWidth && bounds.max.x <= centre.x + halfWidth
                && bounds.min.y >= centre.y - halfHeight && bounds.max.y <= centre.y + halfHeight;
        }
    }
}
