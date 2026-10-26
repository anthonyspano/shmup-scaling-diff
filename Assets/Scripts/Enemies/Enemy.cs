using System;
using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Enemies
{
    // Removes the enemy when its health runs out. Configure its Health so the overshield takes
    // PhotonCannon damage and health takes Melee damage.
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [Tooltip("Destroy the object on death. When off it is deactivated so a spawner can reuse it.")]
        [SerializeField] private bool _destroyOnDeath = true;

        private Health _health;

        public event Action<Enemy> Defeated;

        public Health Health => _health;

        public bool DestroyOnDeath
        {
            get => _destroyOnDeath;
            set => _destroyOnDeath = value;
        }

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
            Defeated?.Invoke(this);

            if (_destroyOnDeath)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
