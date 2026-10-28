using System.Collections;
using ScalingShmup.Game;
using UnityEngine;

namespace ScalingShmup.UI
{
    // Shows a message (e.g. "Game Over!" or "You WIN!") for a few seconds when the GameManager
    // enters a chosen state. Use one per message.
    public class StatePrompt : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;
        [Tooltip("The game state that shows this prompt.")]
        [SerializeField] private GameState _showOnState = GameState.GameOver;
        [Tooltip("The object holding the message text. Hidden until the state is reached.")]
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
            if (state == _showOnState && _prompt != null)
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
