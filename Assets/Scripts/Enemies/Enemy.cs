using System;
using ScalingShmup.Audio;
using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Enemies
{
    // Removes the enemy when its health runs out. Configure its Health so the overshield takes
    // PhotonCannon damage and health takes Melee damage.
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [Tooltip("Remove the object on death. Turn off to leave it in place, e.g. for a boss with a BossDeathSequence.")]
        [SerializeField] private bool _removeOnDeath = true;
        [Tooltip("Destroy the object on death. When off it is deactivated so a spawner can reuse it.")]
        [SerializeField] private bool _destroyOnDeath = true;
        [Tooltip("When off, this enemy never fires. Its movement is unaffected.")]
        [SerializeField] private bool _canShoot = true;
        [Tooltip("Played each time this enemy fires.")]
        [SerializeField] private SoundCue _shootSound = new SoundCue();

        private Health _health;

        public event Action<Enemy> Defeated;

        public Health Health => _health;

        public bool DestroyOnDeath
        {
            get => _destroyOnDeath;
            set => _destroyOnDeath = value;
        }

        public bool CanShoot
        {
            get => _canShoot;
            set => _canShoot = value;
        }

        // Called by whatever fires on this enemy's behalf (e.g. WaveVolley).
        public void PlayShootSound()
        {
            _shootSound.Play();
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

            if (!_removeOnDeath)
            {
                return;
            }

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
