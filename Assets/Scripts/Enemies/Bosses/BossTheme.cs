using ScalingShmup.Combat;
using ScalingShmup.Game;
using UnityEngine;

namespace ScalingShmup.Enemies.Bosses
{
    // Asks the GameManager to switch the music to this boss's theme the moment the boss spawns
    // (is switched on), replacing the stage music. The theme loops until the boss's health runs
    // out, then fades away. Keep the boss switched off in the scene, or the theme would start
    // when the scene loads.
    // Runs before FadeInIntro, which switches every other script off for the boss's fade-in.
    [DefaultExecutionOrder(-50)]
    [RequireComponent(typeof(Health))]
    public class BossTheme : MonoBehaviour
    {
        [SerializeField] private AudioClip _theme;
        [Tooltip("Seconds the theme takes to fade out once the boss's health reaches zero.")]
        [SerializeField, Min(0f)] private float _fadeOutDuration = 2f;
        [Tooltip("Defaults to the GameManager in the scene.")]
        [SerializeField] private GameManager _gameManager;

        private Health _health;
        private bool _hasPlayed;

        private void Awake()
        {
            _health = GetComponent<Health>();

            if (_gameManager == null)
            {
                _gameManager = FindAnyObjectByType<GameManager>();
            }
        }

        private void OnEnable()
        {
            // FadeInIntro switches this script back on after the fade, which isn't a new spawn.
            if (_hasPlayed || _gameManager == null)
            {
                return;
            }

            _gameManager.ChangeTrack(_theme);
            _hasPlayed = true;

            // Subscribed once, for the boss's lifetime: the boss's death sequence switches this
            // script off at the moment of death.
            _health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Died -= OnDied;
            }
        }

        private void OnDied(Health health)
        {
            _gameManager.FadeOutMusic(_fadeOutDuration);
        }
    }
}
