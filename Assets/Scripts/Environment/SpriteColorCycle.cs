using System.Collections;
using UnityEngine;

namespace ScalingShmup.Environment
{
    // Every interval, tints this sprite and all child sprites with the next colour in the list,
    // wrapping back to the first colour after the last. Children are looked up on each change, so
    // tile copies the ParallaxLayer adds at runtime change colour too.
    public class SpriteColorCycle : MonoBehaviour
    {
        [Tooltip("Colours to cycle through, in order. The first is applied when the object starts.")]
        [SerializeField] private Color[] _colors =
        {
            Color.white,
            new Color(0.7f, 0.8f, 1f, 1f),
            new Color(1f, 0.7f, 0.9f, 1f),
            new Color(0.75f, 1f, 0.8f, 1f),
        };
        [Tooltip("Seconds each colour is shown before switching to the next.")]
        [SerializeField, Min(0.1f)] private float _interval = 10f;
        [Tooltip("Seconds to blend into the next colour. 0 switches instantly.")]
        [SerializeField, Min(0f)] private float _transitionDuration = 0f;

        private int _index;

        public int CurrentIndex => _index;

        private void OnEnable()
        {
            if (_colors == null || _colors.Length == 0)
            {
                return;
            }

            _index = 0;
            SetColor(_colors[0]);
            StartCoroutine(Cycle());
        }

        private IEnumerator Cycle()
        {
            while (true)
            {
                yield return new WaitForSeconds(_interval);

                Color from = _colors[_index];
                _index = (_index + 1) % _colors.Length;
                Color to = _colors[_index];

                for (float t = 0f; t < _transitionDuration; t += Time.deltaTime)
                {
                    SetColor(Color.Lerp(from, to, t / _transitionDuration));
                    yield return null;
                }

                SetColor(to);
            }
        }

        private void SetColor(Color color)
        {
            foreach (SpriteRenderer sprite in GetComponentsInChildren<SpriteRenderer>())
            {
                sprite.color = color;
            }
        }
    }
}
