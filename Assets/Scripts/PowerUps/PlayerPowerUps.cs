using System;
using System.Collections;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Holds the player's active power-up. Only one is active at a time: picking up a new one
    // removes the current one first. When the timer runs out the effect is removed, which returns
    // the player to their normal behaviour.
    public class PlayerPowerUps : MonoBehaviour
    {
        private IPowerUpEffect _active;
        private Coroutine _timer;

        public event Action<IPowerUpEffect, float> PowerUpStarted;
        public event Action<IPowerUpEffect> PowerUpEnded;

        public IPowerUpEffect Active => _active;
        public float TimeRemaining { get; private set; }

        public void Activate(IPowerUpEffect effect, float duration)
        {
            if (effect == null)
            {
                return;
            }

            EndActive();

            _active = effect;
            _active.Apply(gameObject);
            PowerUpStarted?.Invoke(_active, duration);
            _timer = StartCoroutine(ExpireAfter(duration));
        }

        // Losing the ship (death, respawn, game over) also ends the power-up.
        private void OnDisable()
        {
            EndActive();
        }

        private IEnumerator ExpireAfter(float duration)
        {
            TimeRemaining = duration;

            while (TimeRemaining > 0f)
            {
                yield return null;
                TimeRemaining -= Time.deltaTime;
            }

            _timer = null;
            EndActive();
        }

        private void EndActive()
        {
            if (_timer != null)
            {
                StopCoroutine(_timer);
                _timer = null;
            }

            if (_active == null)
            {
                return;
            }

            IPowerUpEffect ended = _active;
            _active = null;
            TimeRemaining = 0f;
            ended.Remove(gameObject);
            PowerUpEnded?.Invoke(ended);
        }
    }
}
