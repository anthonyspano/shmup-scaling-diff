using System.Collections;
using ScalingShmup.Game;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Every roll interval, rolls against a spawn chance; on a success it spawns a random power-up
    // prefab at a random point in the left half of the screen.
    public class PowerUpSpawner : MonoBehaviour
    {
        [Tooltip("One of these is picked at random for each spawn.")]
        [SerializeField] private GameObject[] _powerUpPrefabs;
        [Tooltip("Chance of a spawn on each roll, from 0 (never) to 1 (every roll).")]
        [SerializeField, Range(0f, 1f)] private float _powerUpSpawnChance = 0.1f;
        [Tooltip("Seconds between rolls.")]
        [SerializeField, Min(0.1f)] private float _powerUpRollRate = 2f;
        [Tooltip("Keeps spawned power-ups this far inside the screen edges.")]
        [SerializeField, Min(0f)] private float _screenMargin = 0.75f;

        [Header("References")]
        [Tooltip("Defaults to Camera.main. The spawn area follows its view.")]
        [SerializeField] private Camera _camera;
        [Tooltip("Optional. Rolls only happen while the game is being played.")]
        [SerializeField] private GameManager _gameManager;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(RollLoop());
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
