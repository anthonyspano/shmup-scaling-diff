using ScalingShmup.Enemies;
using ScalingShmup.Game;
using UnityEngine;
using UnityEngine.UI;

namespace ScalingShmup.UI
{
    // Shows "Wave x/y": how many waves have spawned out of the WaveSpawner's Max Waves. Hidden
    // once the boss spawns.
    [RequireComponent(typeof(Text))]
    public class WaveCounter : MonoBehaviour
    {
        [SerializeField] private WaveSpawner _waveSpawner;
        [SerializeField] private BossEncounter _bossEncounter;
        [Tooltip("{0} is the waves spawned so far, {1} the maximum.")]
        [SerializeField] private string _format = "Wave {0}/{1}";

        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.WaveStarted += OnWaveStarted;
                Refresh(_waveSpawner.CurrentWave);
            }

            if (_bossEncounter != null)
            {
                _bossEncounter.BossSpawned += OnBossSpawned;
            }
        }

        private void OnDisable()
        {
            if (_waveSpawner != null)
            {
                _waveSpawner.WaveStarted -= OnWaveStarted;
            }

            if (_bossEncounter != null)
            {
                _bossEncounter.BossSpawned -= OnBossSpawned;
            }
        }

        private void OnWaveStarted(int wave)
        {
            Refresh(wave);
        }

        private void OnBossSpawned()
        {
            gameObject.SetActive(false);
        }

        private void Refresh(int wave)
        {
            _text.text = string.Format(_format, wave, _waveSpawner.MaxWaves);
        }
    }
}
