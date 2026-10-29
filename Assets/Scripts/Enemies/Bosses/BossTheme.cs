using ScalingShmup.Game;
using UnityEngine;

namespace ScalingShmup.Enemies.Bosses
{
    // Asks the GameManager to switch the music to this boss's theme the moment the boss spawns
    // (is switched on). The theme loops until the player wins. Keep the boss switched off in the
    // scene, or the theme would start when the scene loads.
    // Runs before FadeInIntro, which switches every other script off for the boss's fade-in.
    [DefaultExecutionOrder(-50)]
    public class BossTheme : MonoBehaviour
    {
        [SerializeField] private AudioClip _theme;
        [Tooltip("Defaults to the GameManager in the scene.")]
        [SerializeField] private GameManager _gameManager;

        private bool _hasPlayed;

        private void OnEnable()
        {
            // FadeInIntro switches this script back on after the fade, which isn't a new spawn.
            if (_hasPlayed)
            {
                return;
            }

            if (_gameManager == null)
            {
                _gameManager = FindAnyObjectByType<GameManager>();
            }

            if (_gameManager != null)
            {
                _gameManager.ChangeTrack(_theme);
                _hasPlayed = true;
            }
        }
    }
}
