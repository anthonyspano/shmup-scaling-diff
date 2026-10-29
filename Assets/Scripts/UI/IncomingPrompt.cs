using System.Collections;
using ScalingShmup.Enemies;
using UnityEngine;

namespace ScalingShmup.UI
{
    // Flashes a warning (e.g. "Enemies Incoming!") for as long as the WaveSpawner waits before its
    // first wave, then hides it.
    public class IncomingPrompt : MonoBehaviour
    {
        [SerializeField] private WaveSpawner _waveSpawner;
        [Tooltip("The object holding the warning text. Hidden except while it flashes.")]
        [SerializeField] private GameObject _prompt;
        [Tooltip("Flashes per second.")]
        [SerializeField, Min(0.1f)] private float _flashRate = 2f;

        private void Awake()
        {
            if (_prompt != null)
            {
                _prompt.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.FirstWaveIncoming += OnFirstWaveIncoming;
            }
        }

        private void OnDisable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.FirstWaveIncoming -= OnFirstWaveIncoming;
            }
        }

        private void OnFirstWaveIncoming(float seconds)
        {
            if (_prompt != null)
            {
                StopAllCoroutines();
                StartCoroutine(Flash(seconds));
            }
        }

        private IEnumerator Flash(float seconds)
        {
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.deltaTime)
            {
                // On for the first half of each flash, off for the second.
                _prompt.SetActive(Mathf.Repeat(elapsed * _flashRate, 1f) < 0.5f);
                yield return null;
            }

            _prompt.SetActive(false);
        }
    }
}
