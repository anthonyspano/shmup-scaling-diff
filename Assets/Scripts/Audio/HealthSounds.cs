using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Audio
{
    // Plays sounds when this object's Health is hit or runs out. Works on the player, enemies,
    // the boss and its turrets.
    [RequireComponent(typeof(Health))]
    public class HealthSounds : MonoBehaviour
    {
        [Tooltip("Played when the object takes damage but survives.")]
        [SerializeField] private SoundCue _hitSound = new SoundCue();
        [Tooltip("Played when the object's health reaches 0.")]
        [SerializeField] private SoundCue _deathSound = new SoundCue();

        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
        }

        private void OnDamaged(Health health, DamageInfo damage)
        {
            // The killing blow plays the death sound instead.
            if (health.IsAlive)
            {
                _hitSound.Play();
            }
            else
                OnDied(health);
        }

        private void OnDied(Health health)
        {
            _deathSound.Play();
        }
    }
}
