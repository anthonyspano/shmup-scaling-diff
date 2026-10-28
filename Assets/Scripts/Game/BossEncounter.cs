using ScalingShmup.Enemies;
using UnityEngine;

namespace ScalingShmup.Game
{
    // Keeps the boss hidden until the WaveSpawner reports that every wave has been defeated, and
    // tells the GameManager the game is won when the boss is defeated.
    public class BossEncounter : MonoBehaviour
    {
        [SerializeField] private WaveSpawner _waveSpawner;
        [Tooltip("The boss object in the scene. It is switched off until the waves are done.")]
        [SerializeField] private GameObject _boss;
        [Tooltip("Defaults to the GameManager on this object.")]
        [SerializeField] private GameManager _gameManager;

        private Enemy _bossEnemy;

        private void Awake()
        {
            if (_gameManager == null)
            {
                _gameManager = GetComponent<GameManager>();
            }

            if (_boss != null)
            {
                _bossEnemy = _boss.GetComponent<Enemy>();
                _boss.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesDefeated += SpawnBoss;
            }

            if (_bossEnemy != null)
            {
                _bossEnemy.Defeated += OnBossDefeated;
            }
        }

        private void OnDisable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesDefeated -= SpawnBoss;
            }

            if (_bossEnemy != null)
            {
                _bossEnemy.Defeated -= OnBossDefeated;
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

        private void OnBossDefeated(Enemy boss)
        {
            if (_gameManager != null)
            {
                _gameManager.WinGame();
            }
        }
    }
}
