using System;
using System.Collections;
using ScalingShmup.Audio;
using ScalingShmup.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScalingShmup.Game
{
    // Owns the game flow (start, respawn, game over, victory), the player's remaining lives and the
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
        [Tooltip("Plays the background music. Defaults to the MusicPlayer on this object.")]
        [SerializeField] private MusicPlayer _music;

        [Header("Game Over")]
        [Tooltip("Seconds between game over and the scene reloading to start a fresh game.")]
        [SerializeField, Min(0f)] private float _restartDelay = 5f;

        [Header("Victory")]
        [Tooltip("Seconds between winning and moving on to the Victory Scene (or reloading this scene if none is set).")]
        [SerializeField, Min(0f)] private float _victoryRestartDelay = 6f;
        [Tooltip("Scene loaded after winning, e.g. Promo. It must be in the build's scene list. Leave empty to start a fresh game.")]
        [SerializeField] private string _victoryScene = "Promo";

        private Coroutine _respawnRoutine;

        public event Action<GameState> StateChanged;
        public event Action<int> LivesChanged;
        public event Action<int> DeathCountChanged;

        public GameState State { get; private set; } = GameState.NotStarted;
        public int Lives { get; private set; }
        public int DeathCount { get; private set; }
        public float RestartDelay => _restartDelay;

        private void Awake()
        {
            if (_music == null)
            {
                _music = GetComponent<MusicPlayer>();
            }
        }

        private void Start()
        {
            if (_startAutomatically)
            {
                StartGame();
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
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

        // Called when the final boss is defeated. Later player deaths are ignored, since State
        // is no longer Playing.
        public void WinGame()
        {
            if (State != GameState.Playing && State != GameState.Respawning)
            {
                return;
            }

            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
                _respawnRoutine = null;
            }

            // The boss theme loops until here.
            if (_music != null)
            {
                _music.Stop();
            }

            SetState(GameState.Victory);
            StartCoroutine(string.IsNullOrEmpty(_victoryScene)
                ? RestartAfterDelay(_victoryRestartDelay)
                : LoadSceneAfterDelay(_victoryScene, _victoryRestartDelay));
        }

        // Swaps the background music, e.g. to the boss theme. The new track loops.
        public void ChangeTrack(AudioClip track)
        {
            if (_music != null)
            {
                _music.ChangeTrack(track, true);
            }
        }

        // Fades the background music to silence, e.g. when the boss's health runs out.
        public void FadeOutMusic(float duration)
        {
            if (_music != null)
            {
                _music.FadeOut(duration);
            }
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
                StartCoroutine(RestartAfterDelay(_restartDelay));
            }
        }

        // Reloading the scene resets every system (waves, pools, HUD) to its starting state.
        private IEnumerator RestartAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(sceneName);
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
