using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Effects
{
    // Plays an explosion where this object dies. Each owner keeps one explosion instance and
    // reuses it, so pooled enemies never instantiate effects during play.
    [RequireComponent(typeof(Health))]
    public class DeathExplosion : MonoBehaviour
    {
        [SerializeField] private ExplosionEffect _explosionPrefab;
        [SerializeField] private AnimationClip _clip;
        [SerializeField, Min(0.01f)] private float _scale = 1f;

        private Health _health;
        private ExplosionEffect _explosion;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;
        }

        private void OnDied(Health health)
        {
            if (_explosionPrefab == null || _clip == null)
            {
                return;
            }

            if (_explosion == null)
            {
                _explosion = Instantiate(_explosionPrefab);
                _explosion.gameObject.SetActive(false);
            }

            // The explosion is a separate root object, so it keeps playing after the owner is
            // deactivated or destroyed.
            _explosion.Play(_clip, transform.position, _scale, OnExplosionFinished);
        }

        private void OnExplosionFinished(ExplosionEffect explosion)
        {
            // An owner destroyed on death leaves its explosion behind; clean that up.
            if (this == null)
            {
                Destroy(explosion.gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_explosion != null && !_explosion.gameObject.activeSelf)
            {
                Destroy(_explosion.gameObject);
            }
        }
    }
}
