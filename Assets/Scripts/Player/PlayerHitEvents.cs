using System;
using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Player
{
    // The single place other systems learn that the player was hit. Each hit also starts a short
    // invulnerability window, during which Health ignores further damage.
    [RequireComponent(typeof(Health))]
    public class PlayerHitEvents : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _invulnerabilityDuration = 1f;

        private Health _health;

        public event Action<DamageInfo> PlayerHit;

        public float InvulnerabilityDuration => _invulnerabilityDuration;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
        }

        private void OnDamaged(Health health, DamageInfo damage)
        {
            // A killing blow is a death, not a hit to recover from.
            if (health.IsAlive)
            {
                health.MakeInvulnerable(_invulnerabilityDuration);
            }

            PlayerHit?.Invoke(damage);
        }
    }
}
