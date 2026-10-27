using System.Collections.Generic;
using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.Enemies
{
    // Fires one volley for a wave: the enemy at the front of the line aims at the player, and every
    // other on-screen enemy in the wave fires along that same direction, so the shots are parallel.
    public class WaveVolley : MonoBehaviour
    {
        [SerializeField] private ProjectilePool _projectilePool;
        [Tooltip("Usually the player. When it is missing, enemies hold fire.")]
        [SerializeField] private Transform _target;
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField] private ProjectileMotion _motion = ProjectileMotion.Straight(6f);
        [Tooltip("Defaults to Camera.main. Only enemies inside its view fire.")]
        [SerializeField] private Camera _camera;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        // wave is in line order; index 0 is the front of the line.
        public void Fire(IReadOnlyList<Enemy> wave)
        {
            if (_target == null || !_target.gameObject.activeInHierarchy || wave.Count == 0)
            {
                return;
            }

            Enemy leader = wave[0];

            // The volley is aimed by the front enemy, so it waits until that enemy is on screen.
            if (!IsOnScreen(leader.transform.position))
            {
                return;
            }

            Vector2 aim = (Vector2)_target.position - (Vector2)leader.transform.position;
            float heading = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;

            // A leader that can't shoot still sets the direction for the rest of the line.
            foreach (Enemy enemy in wave)
            {
                if (enemy.CanShoot && IsOnScreen(enemy.transform.position))
                {
                    var damage = new DamageInfo(_damage, DamageType.EnemyFire, enemy.gameObject);
                    _projectilePool.Get().Launch(enemy.transform.position, heading, _motion, damage);
                }
            }
        }

        private bool IsOnScreen(Vector2 position)
        {
            if (_camera == null || !_camera.orthographic)
            {
                return true;
            }

            Vector2 offset = position - (Vector2)_camera.transform.position;
            return Mathf.Abs(offset.x) <= _camera.orthographicSize * _camera.aspect
                && Mathf.Abs(offset.y) <= _camera.orthographicSize;
        }
    }
}
