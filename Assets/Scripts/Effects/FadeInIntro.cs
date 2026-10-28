using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScalingShmup.Effects
{
    // Each time this object is enabled, it and its children fade from invisible to fully visible.
    // Until the fade finishes, every other script and collider on them is switched off, so nothing
    // moves, shoots or takes damage. They are all switched back on once it is fully visible.
    public class FadeInIntro : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _duration = 3f;

        private readonly List<Behaviour> _paused = new List<Behaviour>();
        private SpriteRenderer[] _sprites;
        private CanvasGroup[] _canvasGroups;

        public bool IsPlaying { get; private set; }

        private void OnEnable()
        {
            PauseBehaviours();
            CollectVisuals();
            SetAlpha(0f);
            StartCoroutine(Fade());
        }

        private void OnDisable()
        {
            // If the object is switched off mid-fade, leave it in a usable state.
            if (IsPlaying)
            {
                SetAlpha(1f);
                ResumeBehaviours();
            }
        }

        private IEnumerator Fade()
        {
            IsPlaying = true;

            for (float elapsed = 0f; elapsed < _duration; elapsed += Time.deltaTime)
            {
                SetAlpha(elapsed / _duration);
                yield return null;
            }

            SetAlpha(1f);
            ResumeBehaviours();
        }

        // UI pieces (health bar canvases and graphics) stay on; they only fade.
        private void PauseBehaviours()
        {
            _paused.Clear();

            foreach (Behaviour behaviour in GetComponentsInChildren<Behaviour>())
            {
                if (behaviour == this || !behaviour.enabled || behaviour is Canvas || behaviour is Graphic || behaviour is CanvasGroup)
                {
                    continue;
                }

                behaviour.enabled = false;
                _paused.Add(behaviour);
            }
        }

        private void ResumeBehaviours()
        {
            IsPlaying = false;

            foreach (Behaviour behaviour in _paused)
            {
                if (behaviour != null)
                {
                    behaviour.enabled = true;
                }
            }

            _paused.Clear();
        }

        private void CollectVisuals()
        {
            _sprites = GetComponentsInChildren<SpriteRenderer>();

            var groups = new List<CanvasGroup>();
            foreach (Canvas canvas in GetComponentsInChildren<Canvas>())
            {
                if (!canvas.TryGetComponent(out CanvasGroup group))
                {
                    group = canvas.gameObject.AddComponent<CanvasGroup>();
                }

                groups.Add(group);
            }

            _canvasGroups = groups.ToArray();
        }

        // Only alpha is changed, so tints set by other scripts are kept.
        private void SetAlpha(float alpha)
        {
            foreach (SpriteRenderer sprite in _sprites)
            {
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = alpha;
                    sprite.color = color;
                }
            }

            foreach (CanvasGroup group in _canvasGroups)
            {
                if (group != null)
                {
                    group.alpha = alpha;
                }
            }
        }
    }
}
