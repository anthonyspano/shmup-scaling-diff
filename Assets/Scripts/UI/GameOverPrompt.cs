using System.Collections;
using ScalingShmup.Game;
using UnityEngine;

namespace ScalingShmup.UI
{
    // Shows the game over message for a few seconds when the GameManager reports game over.
    public class GameOverPrompt : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [Tooltip("The object holding the \"Game Over!\" text. Hidden until game over.")]
        [SerializeField] private GameObject _prompt;
        [SerializeField, Min(0f)] private float _duration = 5f;

        private void Awake()
        {
            if (_prompt != null)
            {
                _prompt.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_gameManager != null)
            {
                _gameManager.StateChanged += OnStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_gameManager != null)
            {
                _gameManager.StateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.GameOver && _prompt != null)
            {
                StartCoroutine(ShowForDuration());
            }
        }

        private IEnumerator ShowForDuration()
        {
            _prompt.SetActive(true);
            yield return new WaitForSeconds(_duration);
            _prompt.SetActive(false);
        }
    }
}
