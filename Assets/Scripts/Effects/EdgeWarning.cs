using System.Collections;
using UnityEngine;

namespace ScalingShmup.Effects
{
    // A flashing marker shown on the edge of the screen where enemies are about to enter. The
    // sprite is authored as a semicircle with its flat side down and its dome pointing up (+Y),
    // so it is rotated to point into the screen, with its flat side lying on the edge.
    [RequireComponent(typeof(SpriteRenderer))]
    public class EdgeWarning : MonoBehaviour
    {
        [Tooltip("Flashes per second.")]
        [SerializeField, Min(0.1f)] private float _flashRate = 6f;

        private SpriteRenderer _spriteRenderer;

        public bool IsShowing { get; private set; }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // inward: the direction pointing from the edge into the screen.
        public void Show(Vector2 position, Vector2 inward, float duration)
        {
            transform.SetPositionAndRotation(position, Quaternion.FromToRotation(Vector3.up, inward));
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(Flash(duration));
        }

        private IEnumerator Flash(float duration)
        {
            IsShowing = true;

            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                _spriteRenderer.enabled = Mathf.Repeat(elapsed * _flashRate, 1f) < 0.5f;
                yield return null;
            }

            IsShowing = false;
            gameObject.SetActive(false);
        }
    }
}
