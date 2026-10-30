using System;
using System.Collections;
using System.Collections.Generic;
using ScalingShmup.Effects;
using ScalingShmup.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace ScalingShmup.Enemies
{
    // Spawns each wave just off the right edge of the screen. Every enemy in a wave leaves from the
    // same point and flies toward the same left-hand corner, so the wave forms a single-file line.
    // Enemies that survive a run wait off screen, then make another run in the same line from a
    // different edge until they are all defeated. Waves run independently: a new wave starts on a
    // timer whether or not earlier waves are still alive.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private Enemy _enemyPrefab;

        [Header("Waves")]
        [SerializeField, Min(1)] private int _waveSize = 8;
        [Tooltip("Enemies added to each wave after the first.")]
        [SerializeField, Min(0)] private int _waveSizeIncrease = 2;
        [Tooltip("Enemies spawned per second within a wave. Lower values leave bigger gaps in the line.")]
        [SerializeField, Min(0.1f)] private float _spawnRate = 4f;
        [Tooltip("Seconds from the last enemy of one wave spawning to the next wave starting.")]
        [SerializeField, Min(0f)] private float _timeBetweenWaves = 2f;
        [Tooltip("No new waves spawn after this many. Waves already out keep making runs until defeated.")]
        [SerializeField, Min(1)] private int _maxWaves = 8;
        [SerializeField] private bool _startOnPlay = true;
        [Tooltip("Seconds of warning (the \"Enemies Incoming!\" prompt) before the first wave spawns.")]
        [SerializeField, Min(0f)] private float _firstWaveDelay = 5f;

        [Header("Path")]
        [SerializeField] private WaveTargetCorner _targetCorner = WaveTargetCorner.Random;
        [Tooltip("How far beyond the screen edge enemies appear.")]
        [SerializeField, Min(0f)] private float _spawnOffset = 1f;
        [Tooltip("Spawn height is picked per wave within this fraction of the screen height, centred vertically.")]
        [SerializeField, Range(0f, 1f)] private float _spawnHeightRange = 0.6f;
        [Tooltip("Seconds survivors wait off screen between one run and the next.")]
        [SerializeField, Min(0f)] private float _delayBetweenRuns = 1.5f;

        [Header("Entry warning")]
        [Tooltip("Optional. Flashing marker shown on the screen edge where each run is about to enter.")]
        [SerializeField] private EdgeWarning _edgeWarningPrefab;
        [Tooltip("Seconds the warning shows before the first ship of a run starts to appear on screen.")]
        [SerializeField, Min(0f)] private float _warningLeadTime = 1f;

        [Header("Shooting")]
        [Tooltip("Optional. Each enemy fires once per run, as soon as it is fully on screen.")]
        [SerializeField] private WaveVolley _volley;

        [Header("References")]
        [Tooltip("Defaults to Camera.main. Spawn points and corners follow its view.")]
        [SerializeField] private Camera _camera;
        [Tooltip("Optional. Spawning stops when the game is over.")]
        [SerializeField] private GameManager _gameManager;

        // Each wave's living enemies, in line order. The front of the line is index 0.
        private readonly List<List<Enemy>> _waves = new List<List<Enemy>>();
        private readonly Dictionary<Enemy, List<Enemy>> _waveOf = new Dictionary<Enemy, List<Enemy>>();
        private readonly List<EdgeWarning> _warnings = new List<EdgeWarning>();
        private ObjectPool<Enemy> _pool;
        private bool _isRunning;

        public event Action<int> WaveStarted;
        public event Action<int> WaveCleared;
        // Raised when waves are started, with the seconds left before the first one spawns.
        public event Action<float> FirstWaveIncoming;
        // Raised once every one of the Max Waves waves has spawned and been defeated.
        public event Action AllWavesDefeated;

        public int CurrentWave { get; private set; }
        public int MaxWaves => _maxWaves;
        public int ActiveWaveCount => _waves.Count;
        public int AliveCount => _waveOf.Count;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            _pool = new ObjectPool<Enemy>(
                CreateEnemy,
                actionOnGet: null,
                actionOnRelease: enemy => enemy.gameObject.SetActive(false),
                // Unity clears pools when Play mode ends, after the scene has already destroyed them.
                actionOnDestroy: enemy =>
                {
                    if (enemy != null)
                    {
                        Destroy(enemy.gameObject);
                    }
                },
                collectionCheck: false);
        }

        private void OnEnable()
        {
            if (_gameManager != null)
            {
                _gameManager.StateChanged += OnGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.StateChanged -= OnGameStateChanged;
            }

            _isRunning = false;
        }

        private void Start()
        {
            if (_startOnPlay)
            {
                StartWaves();
            }
        }

        public void StartWaves()
        {
            StopWaves();
            CurrentWave = 0;
            _isRunning = true;
            StartCoroutine(StartWavesOnTimer());
        }

        // Stops new waves, runs and volleys. Enemies already on screen finish their current run.
        public void StopWaves()
        {
            _isRunning = false;
            StopAllCoroutines();
        }

        private IEnumerator StartWavesOnTimer()
        {
            if (_firstWaveDelay > 0f)
            {
                FirstWaveIncoming?.Invoke(_firstWaveDelay);
                yield return new WaitForSeconds(_firstWaveDelay);
            }

            while (_isRunning && CurrentWave < _maxWaves)
            {
                CurrentWave++;
                var wave = new List<Enemy>();
                _waves.Add(wave);
                WaveStarted?.Invoke(CurrentWave);

                // Watch from the first spawn, so the front enemies fire the moment they're fully in view.
                if (_volley != null)
                {
                    StartCoroutine(WatchForShots(wave));
                }

                yield return SpawnWave(wave, CurrentWave);
                StartCoroutine(RepeatRuns(wave, CurrentWave));

                yield return new WaitForSeconds(_timeBetweenWaves);
            }
        }

        private IEnumerator SpawnWave(List<Enemy> wave, int waveNumber)
        {
            var spawnDelay = new WaitForSeconds(1f / _spawnRate);
            int size = _waveSize + (waveNumber - 1) * _waveSizeIncrease;
            bool towardTop = PickTopCorner(waveNumber);
            float startAlong = UnityEngine.Random.Range(-0.5f, 0.5f) * _spawnHeightRange;

            yield return WarnBeforeEntry(ScreenEdge.Right, GetEdgePoint(ScreenEdge.Right, startAlong, _spawnOffset), GetCorner(towardTop));

            for (int i = 0; i < size; i++)
            {
                // Recomputed per enemy so the path follows the camera if the view changes.
                SpawnEnemy(wave, GetEdgePoint(ScreenEdge.Right, startAlong, _spawnOffset), GetCorner(towardTop));
                yield return spawnDelay;
            }
        }

        private IEnumerator RepeatRuns(List<Enemy> wave, int waveNumber)
        {
            var spawnDelay = new WaitForSeconds(1f / _spawnRate);
            ScreenEdge lastEdge = ScreenEdge.Right;

            while (true)
            {
                yield return new WaitUntil(() => RunFinished(wave));

                if (wave.Count == 0)
                {
                    break;
                }

                yield return new WaitForSeconds(_delayBetweenRuns);

                if (wave.Count == 0)
                {
                    break;
                }

                lastEdge = PickEdgeOtherThan(lastEdge);
                yield return LaunchRun(wave, lastEdge, spawnDelay);
            }

            _waves.Remove(wave);
            WaveCleared?.Invoke(waveNumber);

            // A wave is only removed after it has fully spawned, so this means the last one is done.
            if (CurrentWave >= _maxWaves && _waves.Count == 0)
            {
                AllWavesDefeated?.Invoke();
            }
        }

        private IEnumerator WatchForShots(List<Enemy> wave)
        {
            _volley.StartRun(wave);

            while (_waves.Contains(wave))
            {
                _volley.UpdateWave(wave);
                yield return null;
            }

            _volley.EndWave(wave);
        }

        // Survivors keep their order in the line, so the formation is the same on every run.
        private IEnumerator LaunchRun(List<Enemy> wave, ScreenEdge fromEdge, WaitForSeconds spawnDelay)
        {
            float startAlong = UnityEngine.Random.Range(-0.5f, 0.5f) * _spawnHeightRange;
            float targetAlong = UnityEngine.Random.Range(-0.5f, 0.5f);
            var survivors = new List<Enemy>(wave);

            yield return WarnBeforeEntry(fromEdge, GetEdgePoint(fromEdge, startAlong, _spawnOffset), GetEdgePoint(Opposite(fromEdge), targetAlong, 0f));

            if (_volley != null)
            {
                _volley.StartRun(wave);
            }

            foreach (Enemy enemy in survivors)
            {
                // Skip any enemy killed while earlier ones were launching.
                if (!wave.Contains(enemy))
                {
                    continue;
                }

                if (enemy.TryGetComponent(out EnemyLineMover mover))
                {
                    mover.Launch(GetEdgePoint(fromEdge, startAlong, _spawnOffset), GetEdgePoint(Opposite(fromEdge), targetAlong, 0f));
                }

                yield return spawnDelay;
            }
        }

        // Shows the edge warning where the run's first ship will cross into view, then holds the run
        // back so that ship starts to appear Warning Lead Time seconds after the warning appears.
        private IEnumerator WarnBeforeEntry(ScreenEdge edge, Vector2 start, Vector2 target)
        {
            if (_edgeWarningPrefab == null || _warningLeadTime <= 0f)
            {
                yield break;
            }

            Vector2 direction = (target - start).normalized;
            GetView(out Vector2 centre, out Vector2 halfSize);
            Vector2 inward = InwardNormal(edge);

            // Distance along the path until the ship's centre reaches the edge line.
            float distanceFromEdge = edge == ScreenEdge.Left || edge == ScreenEdge.Right
                ? Mathf.Abs(start.x - (centre.x - inward.x * halfSize.x))
                : Mathf.Abs(start.y - (centre.y - inward.y * halfSize.y));
            float speedTowardScreen = Mathf.Max(0.01f, Vector2.Dot(direction, inward));
            float centreDistance = distanceFromEdge / speedTowardScreen;
            Vector2 crossing = start + direction * centreDistance;

            // The ship shows up as soon as its leading edge crosses, half a sprite before its centre.
            float leadingEdgeDistance = Mathf.Max(0f, centreDistance - EnemyHalfSize() / speedTowardScreen);
            float travelTime = leadingEdgeDistance / Mathf.Max(0.01f, EnemySpeed());

            GetWarning().Show(crossing, inward, _warningLeadTime);
            yield return new WaitForSeconds(Mathf.Max(0f, _warningLeadTime - travelTime));
        }

        private EdgeWarning GetWarning()
        {
            foreach (EdgeWarning warning in _warnings)
            {
                if (!warning.gameObject.activeSelf)
                {
                    return warning;
                }
            }

            EdgeWarning created = Instantiate(_edgeWarningPrefab, transform);
            _warnings.Add(created);
            return created;
        }

        private float EnemySpeed()
        {
            return _enemyPrefab.TryGetComponent(out EnemyLineMover mover) ? mover.Speed : 4f;
        }

        private float EnemyHalfSize()
        {
            return _enemyPrefab.TryGetComponent(out SpriteRenderer sprite) && sprite.sprite != null
                ? Mathf.Max(sprite.sprite.bounds.extents.x, sprite.sprite.bounds.extents.y) * _enemyPrefab.transform.localScale.x
                : 0f;
        }

        private static Vector2 InwardNormal(ScreenEdge edge)
        {
            switch (edge)
            {
                case ScreenEdge.Left:
                    return Vector2.right;
                case ScreenEdge.Right:
                    return Vector2.left;
                case ScreenEdge.Top:
                    return Vector2.down;
                default:
                    return Vector2.up;
            }
        }

        private static bool RunFinished(List<Enemy> wave)
        {
            foreach (Enemy enemy in wave)
            {
                if (enemy.TryGetComponent(out EnemyLineMover mover) && mover.IsMoving)
                {
                    return false;
                }
            }

            return true;
        }

        private bool PickTopCorner(int wave)
        {
            switch (_targetCorner)
            {
                case WaveTargetCorner.TopLeft:
                    return true;
                case WaveTargetCorner.BottomLeft:
                    return false;
                case WaveTargetCorner.Alternate:
                    return wave % 2 == 1;
                default:
                    return UnityEngine.Random.value < 0.5f;
            }
        }

        private static ScreenEdge PickEdgeOtherThan(ScreenEdge previous)
        {
            // Offset by 1 to 3 so the result is always a different edge.
            return (ScreenEdge)(((int)previous + UnityEngine.Random.Range(1, 4)) % 4);
        }

        private static ScreenEdge Opposite(ScreenEdge edge)
        {
            switch (edge)
            {
                case ScreenEdge.Left:
                    return ScreenEdge.Right;
                case ScreenEdge.Right:
                    return ScreenEdge.Left;
                case ScreenEdge.Top:
                    return ScreenEdge.Bottom;
                default:
                    return ScreenEdge.Top;
            }
        }

        private void SpawnEnemy(List<Enemy> wave, Vector2 spawnPosition, Vector2 target)
        {
            Enemy enemy = _pool.Get();
            enemy.gameObject.SetActive(true);
            enemy.Health.RestoreToFull();

            if (enemy.TryGetComponent(out EnemyLineMover mover))
            {
                mover.Launch(spawnPosition, target);
            }
            else
            {
                enemy.transform.position = spawnPosition;
            }

            wave.Add(enemy);
            _waveOf[enemy] = wave;
        }

        // along: position along the edge as a fraction of its length, from -0.5 to 0.5.
        // outside: distance beyond the edge, so spawn points start off screen.
        private Vector2 GetEdgePoint(ScreenEdge edge, float along, float outside)
        {
            GetView(out Vector2 centre, out Vector2 halfSize);

            switch (edge)
            {
                case ScreenEdge.Left:
                    return new Vector2(centre.x - halfSize.x - outside, centre.y + along * 2f * halfSize.y);
                case ScreenEdge.Right:
                    return new Vector2(centre.x + halfSize.x + outside, centre.y + along * 2f * halfSize.y);
                case ScreenEdge.Top:
                    return new Vector2(centre.x + along * 2f * halfSize.x, centre.y + halfSize.y + outside);
                default:
                    return new Vector2(centre.x + along * 2f * halfSize.x, centre.y - halfSize.y - outside);
            }
        }

        private Vector2 GetCorner(bool top)
        {
            GetView(out Vector2 centre, out Vector2 halfSize);
            return new Vector2(centre.x - halfSize.x, centre.y + (top ? halfSize.y : -halfSize.y));
        }

        private void GetView(out Vector2 centre, out Vector2 halfSize)
        {
            if (_camera == null || !_camera.orthographic)
            {
                centre = transform.position;
                halfSize = new Vector2(8f, 5f);
                return;
            }

            centre = _camera.transform.position;
            halfSize = new Vector2(_camera.orthographicSize * _camera.aspect, _camera.orthographicSize);
        }

        private Enemy CreateEnemy()
        {
            Enemy enemy = Instantiate(_enemyPrefab, transform);
            // The pool owns the enemy's lifetime, so it must not destroy itself on death.
            enemy.DestroyOnDeath = false;
            enemy.Defeated += Reclaim;
            return enemy;
        }

        // Only defeated enemies return to the pool; ones that leave the screen come back for another run.
        private void Reclaim(Enemy enemy)
        {
            if (_waveOf.TryGetValue(enemy, out List<Enemy> wave))
            {
                wave.Remove(enemy);
                _waveOf.Remove(enemy);
            }

            _pool.Release(enemy);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                StopWaves();
            }
        }
    }
}
