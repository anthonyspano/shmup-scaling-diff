using ScalingShmup.Enemies;
using UnityEngine;

namespace ScalingShmup.Game
{
    // Keeps the boss hidden until the WaveSpawner reports that every wave has been defeated.
    public class BossEncounter : MonoBehaviour
    {
        [SerializeField] private WaveSpawner _waveSpawner;
        [Tooltip("The boss object in the scene. It is switched off until the waves are done.")]
        [SerializeField] private GameObject _boss;

        private void Awake()
        {
            if (_boss != null)
            {
                _boss.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesDefeated += SpawnBoss;
            }
        }

        private void OnDisable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesDefeated -= SpawnBoss;
            }
        }

        // With the WaveSpawner switched off (e.g. while testing the boss), there are no waves to
        // wait for, so the boss appears straight away.
        private void Start()
        {
            if (_waveSpawner == null || !_waveSpawner.isActiveAndEnabled)
            {
                SpawnBoss();
            }
        }

        private void SpawnBoss()
        {
            if (_boss != null)
            {
                _boss.SetActive(true);
            }
        }
    }
}
