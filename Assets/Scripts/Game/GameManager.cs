using System;
using System.Collections;
using ScalingShmup.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScalingShmup.Game
{
    // Owns the game flow (start, respawn, game over), the player's remaining lives and the
    // number of times the player has died.
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Health _player;
        [SerializeField] private Transform _playerSpawnPoint;
        [Tooltip("Ships the player starts with, including the one in play.")]
        [SerializeField, Min(1)] private int _startingLives = 3;
        [SerializeField, Min(0f)] private float _respawnDelay = 1.5f;
        [SerializeField, Min(0f)] private float _respawnInvulnerability = 2f;
        [SerializeField] private bool _startAutomatically = true;

        [Header("Game Over")]
        [Tooltip("Seconds between game over and the scene reloading to start a fresh game.")]
        [SerializeField, Min(0f)] private float _restartDelay = 5f;

        private Coroutine _respawnRoutine;

        public event Action<GameState> StateChanged;
        public event Action<int> LivesChanged;
        public event Action<int> DeathCountChanged;

        public GameState State { get; private set; } = GameState.NotStarted;
        public int Lives { get; private set; }
        public int DeathCount { get; private set; }
        public float RestartDelay => _restartDelay;

        private void Start()
        {
            if (_startAutomatically)
            {
                StartGame();
            }
        }

        private void OnEnable()
        {
            _player.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            // The player is destroyed on game over, so it may already be gone.
            if (_player != null)
            {
                _player.Died -= OnPlayerDied;
            }
        }

        public void StartGame()
        {
            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
                _respawnRoutine = null;
            }

            SetLives(_startingLives);
            SetDeathCount(0);
            SpawnPlayer();
            SetState(GameState.Playing);
        }

        public void AddLife()
        {
            if (State != GameState.GameOver)
            {
                SetLives(Lives + 1);
            }
        }

        private void OnPlayerDied(Health player)
        {
            if (State != GameState.Playing)
            {
                return;
            }

            SetDeathCount(DeathCount + 1);
            SetLives(Lives - 1);

            if (Lives > 0)
            {
                _player.gameObject.SetActive(false);
                _respawnRoutine = StartCoroutine(RespawnAfterDelay());
            }
            else
            {
                _player.Died -= OnPlayerDied;
                Destroy(_player.gameObject);
                SetState(GameState.GameOver);
                StartCoroutine(RestartAfterDelay());
            }
        }

        // Reloading the scene resets every system (waves, pools, HUD) to its starting state.
        private IEnumerator RestartAfterDelay()
        {
            yield return new WaitForSeconds(_restartDelay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator RespawnAfterDelay()
        {
            SetState(GameState.Respawning);
            yield return new WaitForSeconds(_respawnDelay);

            _respawnRoutine = null;
            SpawnPlayer();
            SetState(GameState.Playing);
        }

        private void SpawnPlayer()
        {
            if (_playerSpawnPoint != null)
            {
                _player.transform.SetPositionAndRotation(_playerSpawnPoint.position, _playerSpawnPoint.rotation);
            }

            _player.gameObject.SetActive(true);
            _player.RestoreToFull();
            _player.MakeInvulnerable(_respawnInvulnerability);
        }

        private void SetState(GameState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(state);
        }

        private void SetLives(int lives)
        {
            Lives = Mathf.Max(0, lives);
            LivesChanged?.Invoke(Lives);
        }

        private void SetDeathCount(int deathCount)
        {
            DeathCount = deathCount;
            DeathCountChanged?.Invoke(DeathCount);
        }
    }
}
