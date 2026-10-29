using System;
using UnityEngine;

namespace ScalingShmup.Combat
{
    // Two independent pools: overshield and health. Which damage types reach each pool is
    // configured per object, so enemies (overshield <- photon, health <- melee) and the player
    // share this one component.
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        // Defaults describe an enemy: a small health pool behind a much larger overshield.
        [SerializeField, Min(1f)] private float _maxHealth = 2f;
        [SerializeField] private DamageType _healthDamagedBy = DamageType.Melee;

        [Header("Overshield")]
        [SerializeField, Min(0f)] private float _maxOvershield = 10f;
        [SerializeField] private DamageType _overshieldDamagedBy = DamageType.PhotonCannon;
        [Tooltip("While the overshield is up, health cannot be damaged.")]
        [SerializeField] private bool _overshieldBlocksHealthDamage = false;

        private float _currentHealth;
        private float _currentOvershield;
        private float _invulnerableUntil;

        public event Action<Health, DamageInfo> Damaged;
        public event Action<Health> Died;
        public event Action<Health> Restored;
        public event Action<float, float> HealthChanged;
        public event Action<float, float> OvershieldChanged;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float MaxOvershield => _maxOvershield;
        public float CurrentOvershield => _currentOvershield;
        public bool HasOvershield => _currentOvershield > 0f;
        public bool IsAlive => _currentHealth > 0f;
        public bool IsInvulnerable => Time.time < _invulnerableUntil;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            _currentOvershield = _maxOvershield;
        }

        public bool TakeDamage(DamageInfo damage)
        {
            if (!IsAlive || IsInvulnerable || damage.Amount <= 0f)
            {
                return false;
            }

            bool applied = false;

            if (HasOvershield)
            {
                if (Accepts(_overshieldDamagedBy, damage.Type))
                {
                    SetOvershield(_currentOvershield - damage.Amount);
                    applied = true;
                }

                if (_overshieldBlocksHealthDamage)
                {
                    return NotifyDamaged(applied, damage);
                }
            }

            if (Accepts(_healthDamagedBy, damage.Type))
            {
                SetHealth(_currentHealth - damage.Amount);
                applied = true;
            }

            NotifyDamaged(applied, damage);

            if (!IsAlive)
            {
                Died?.Invoke(this);
            }

            return applied;
        }

        public void RestoreToFull()
        {
            SetHealth(_maxHealth);
            SetOvershield(_maxOvershield);
            Restored?.Invoke(this);
        }

        // Adds overshield up to the maximum, e.g. from a pickup. Returns how much was actually added.
        public float AddOvershield(float amount)
        {
            float before = _currentOvershield;
            SetOvershield(_currentOvershield + Mathf.Max(0f, amount));
            return _currentOvershield - before;
        }

        public void MakeInvulnerable(float duration)
        {
            _invulnerableUntil = Mathf.Max(_invulnerableUntil, Time.time + duration);
        }

        private static bool Accepts(DamageType acceptedTypes, DamageType incoming)
        {
            return (acceptedTypes & incoming) != 0;
        }

        private bool NotifyDamaged(bool applied, DamageInfo damage)
        {
            if (applied)
            {
                Damaged?.Invoke(this, damage);
            }

            return applied;
        }

        private void SetHealth(float value)
        {
            _currentHealth = Mathf.Clamp(value, 0f, _maxHealth);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        private void SetOvershield(float value)
        {
            _currentOvershield = Mathf.Clamp(value, 0f, _maxOvershield);
            OvershieldChanged?.Invoke(_currentOvershield, _maxOvershield);
        }
    }
}
