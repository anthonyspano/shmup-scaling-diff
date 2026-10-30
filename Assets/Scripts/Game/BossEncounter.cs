using System.Collections;
using ScalingShmup.Enemies;
using ScalingShmup.Enemies.Bosses;
using UnityEngine;

namespace ScalingShmup.Game
{
    // Keeps the boss hidden until the WaveSpawner reports that every wave has been defeated, and
    // tells the GameManager the game is won when the boss is destroyed.
    public class BossEncounter : MonoBehaviour
    {
        [SerializeField] private WaveSpawner _waveSpawner;
        [Tooltip("The boss object in the scene. It is switched off until the waves are done.")]
        [SerializeField] private GameObject _boss;
        [Tooltip("Defaults to the GameManager on this object.")]
        [SerializeField] private GameManager _gameManager;

        private Enemy _bossEnemy;
        private BossDeathSequence _bossDeathSequence;

        private void Awake()
        {
            if (_gameManager == null)
            {
                _gameManager = GetComponent<GameManager>();
            }

            if (_boss != null)
            {
                _bossEnemy = _boss.GetComponent<Enemy>();
                _bossDeathSequence = _boss.GetComponent<BossDeathSequence>();
                _boss.SetActive(false);
            }
        }

        // A boss with a death sequence wins the game once its last explosion has played out;
        // otherwise it wins the moment the boss is defeated.
        private void OnEnable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.AllWavesDefeated += SpawnBoss;
            }

            if (_bossDeathSequence != null)
            {
                _bossDeathSequence.Finished += OnBossDestroyed;
            }
            else if (_bossEnemy != null)
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

            if (_bossDeathSequence != null)
            {
                _bossDeathSequence.Finished -= OnBossDestroyed;
            }

            if (_bossEnemy != null)
            {
                _bossEnemy.Defeated -= OnBossDefeated;
            }
        }

        // The boss is gone as its last explosion fires; the win waits until that explosion ends.
        private void OnBossDestroyed(float secondsUntilLastExplosionEnds)
        {
            StartCoroutine(WinAfter(secondsUntilLastExplosionEnds));
        }

        private IEnumerator WinAfter(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_gameManager != null)
            {
                _gameManager.WinGame();
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
