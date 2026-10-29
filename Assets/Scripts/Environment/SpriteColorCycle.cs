using System.Collections;
using UnityEngine;

namespace ScalingShmup.Environment
{
    // Every interval, tints this sprite and all child sprites with the next colour in the list,
    // wrapping back to the first colour after the last. The change blends around the colour wheel
    // (hue) rather than straight through RGB, so it passes through the hues in between.
    // Children are looked up on each change, so tile copies the ParallaxLayer adds at runtime
    // change colour too.
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
        [Tooltip("Seconds spent blending around the colour wheel into the next colour. 0 switches instantly.")]
        [SerializeField, Min(0f)] private float _transitionDuration = 3f;

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
                // The blend is part of the interval, so a new colour is reached every Interval seconds.
                yield return new WaitForSeconds(Mathf.Max(0f, _interval - _transitionDuration));

                Color from = _colors[_index];
                _index = (_index + 1) % _colors.Length;
                Color to = _colors[_index];

                for (float t = 0f; t < _transitionDuration; t += Time.deltaTime)
                {
                    SetColor(LerpAroundColorWheel(from, to, t / _transitionDuration));
                    yield return null;
                }

                SetColor(to);
            }
        }

        // Blends hue the short way around the wheel, and saturation, brightness and alpha directly.
        private static Color LerpAroundColorWheel(Color from, Color to, float t)
        {
            Color.RGBToHSV(from, out float h1, out float s1, out float v1);
            Color.RGBToHSV(to, out float h2, out float s2, out float v2);

            // Greys and white have no real hue, so borrow the other colour's to avoid a pointless sweep.
            const float greyThreshold = 0.01f;
            if (s1 < greyThreshold) h1 = h2;
            if (s2 < greyThreshold) h2 = h1;

            float hue = Mathf.Repeat(h1 + Mathf.DeltaAngle(h1 * 360f, h2 * 360f) / 360f * t, 1f);
            Color color = Color.HSVToRGB(hue, Mathf.Lerp(s1, s2, t), Mathf.Lerp(v1, v2, t));
            color.a = Mathf.Lerp(from.a, to.a, t);
            return color;
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
