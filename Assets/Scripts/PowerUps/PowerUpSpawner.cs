using System.Collections;
using ScalingShmup.Game;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Every roll interval, rolls against a spawn chance; on a success it spawns a random power-up
    // prefab at a random point in the left half of the screen. Each player death raises the chance,
    // capped at 100%, to help a struggling player. The raised chance carries over when a game over
    // restarts the scene, and goes back to the Inspector value once the player wins.
    // AutoStaticsCleanup clears the carried chance whenever Play mode starts.
    [AutoStaticsCleanup]
    public partial class PowerUpSpawner : MonoBehaviour
    {
        // Survives scene reloads; null means "use the Inspector value".
        private static float? s_carriedChance;

        [Tooltip("One of these is picked at random for each spawn.")]
        [SerializeField] private GameObject[] _powerUpPrefabs;
        [Tooltip("Chance of a spawn on each roll, from 0 (never) to 1 (every roll). The value set here is the starting chance; in Play mode it shows the live chance as deaths raise it.")]
        [SerializeField, Range(0f, 1f)] private float _powerUpSpawnChance = 0.1f;
        [Tooltip("Added to the spawn chance each time the player dies. 0.25 adds 25 percentage points.")]
        [SerializeField, Range(0f, 1f)] private float _chanceIncreasePerDeath = 0.25f;
        [Tooltip("Seconds between rolls.")]
        [SerializeField, Min(0.1f)] private float _powerUpRollRate = 2f;
        [Tooltip("Keeps spawned power-ups this far inside the screen edges.")]
        [SerializeField, Min(0f)] private float _screenMargin = 0.75f;

        [Header("References")]
        [Tooltip("Defaults to Camera.main. The spawn area follows its view.")]
        [SerializeField] private Camera _camera;
        [Tooltip("Optional. Rolls only happen while the game is being played.")]
        [SerializeField] private GameManager _gameManager;

        // The starting chance from the scene, restored when the player wins.
        private float _defaultChance;
        private int _deathsCounted;

        public float CurrentSpawnChance => _powerUpSpawnChance;

        private void Awake()
        {
            _defaultChance = _powerUpSpawnChance;

            if (s_carriedChance.HasValue)
            {
                _powerUpSpawnChance = s_carriedChance.Value;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(RollLoop());

            if (_gameManager != null)
            {
                _gameManager.DeathCountChanged += OnDeathCountChanged;
                _gameManager.StateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.DeathCountChanged -= OnDeathCountChanged;
                _gameManager.StateChanged -= OnGameStateChanged;
            }
        }

        // Winning ends the run, so the next game starts back at the normal chance.
        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Victory)
            {
                s_carriedChance = null;
                _powerUpSpawnChance = _defaultChance;
            }
        }

        // DeathCountChanged also fires when the count is reset to 0 at the start of a game, so only
        // count increases.
        private void OnDeathCountChanged(int deathCount)
        {
            if (deathCount > _deathsCounted)
            {
                int newDeaths = deathCount - _deathsCounted;
                _powerUpSpawnChance = Mathf.Min(1f, _powerUpSpawnChance + newDeaths * _chanceIncreasePerDeath);
                s_carriedChance = _powerUpSpawnChance;
            }

            _deathsCounted = deathCount;
        }

        private IEnumerator RollLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_powerUpRollRate);

                bool playing = _gameManager == null || _gameManager.State == GameState.Playing;

                if (playing && Random.value < _powerUpSpawnChance)
                {
                    Spawn();
                }
            }
        }

        private void Spawn()
        {
            if (_powerUpPrefabs == null || _powerUpPrefabs.Length == 0)
            {
                return;
            }

            GameObject prefab = _powerUpPrefabs[Random.Range(0, _powerUpPrefabs.Length)];

            // Power-ups are rare (one per few rolls at most), so instantiating is fine here.
            if (prefab != null)
            {
                Instantiate(prefab, RandomPointLeftOfCentre(), Quaternion.identity);
            }
        }

        private Vector2 RandomPointLeftOfCentre()
        {
            Vector2 centre = _camera != null ? (Vector2)_camera.transform.position : Vector2.zero;
            float halfHeight = _camera != null && _camera.orthographic ? _camera.orthographicSize : 5f;
            float halfWidth = halfHeight * (_camera != null ? _camera.aspect : 16f / 9f);

            float x = Random.Range(centre.x - halfWidth + _screenMargin, centre.x);
            float y = Random.Range(centre.y - halfHeight + _screenMargin, centre.y + halfHeight - _screenMargin);
            return new Vector2(x, y);
        }
    }
}
