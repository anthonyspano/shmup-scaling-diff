using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.UI
{
    // Shows one of a Health component's pools as a bar. The fill is sized through its anchors,
    // so it needs no sprite and stays correct at any resolution.
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private HealthBarSource _source = HealthBarSource.Overshield;
        [Tooltip("Child RectTransform that stretches from the left edge to show the current amount.")]
        [SerializeField] private RectTransform _fill;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        // Health only raises events on change, so read the starting values once it has woken up.
        private void Start()
        {
            Refresh();
        }

        public void SetTarget(Health health)
        {
            Unsubscribe();
            _health = health;
            Subscribe();
            Refresh();
        }

        private void Subscribe()
        {
            if (_health != null)
            {
                _health.HealthChanged += OnHealthChanged;
                _health.OvershieldChanged += OnOvershieldChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_health != null)
            {
                _health.HealthChanged -= OnHealthChanged;
                _health.OvershieldChanged -= OnOvershieldChanged;
            }
        }

        private void Refresh()
        {
            if (_health == null)
            {
                return;
            }

            if (_source == HealthBarSource.Health)
            {
                SetFill(_health.CurrentHealth, _health.MaxHealth);
            }
            else
            {
                SetFill(_health.CurrentOvershield, _health.MaxOvershield);
            }
        }

        private void OnHealthChanged(float current, float max)
        {
            if (_source == HealthBarSource.Health)
            {
                SetFill(current, max);
            }
        }

        private void OnOvershieldChanged(float current, float max)
        {
            if (_source == HealthBarSource.Overshield)
            {
                SetFill(current, max);
            }
        }

        private void SetFill(float current, float max)
        {
            if (_fill == null)
            {
                return;
            }

            float fraction = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            _fill.anchorMin = new Vector2(0f, _fill.anchorMin.y);
            _fill.anchorMax = new Vector2(fraction, _fill.anchorMax.y);
        }
    }
}
