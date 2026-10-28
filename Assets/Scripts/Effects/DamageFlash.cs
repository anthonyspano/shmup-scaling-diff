using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.Effects
{
    // Flashes the sprite red while the object keeps taking damage, and stops once it has gone
    // a set time without being hit.
    [RequireComponent(typeof(Health), typeof(SpriteRenderer))]
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private Color _flashColor = Color.red;
        [Tooltip("Flashes per second while flashing.")]
        [SerializeField, Min(0.1f)] private float _flashRate = 10f;
        [Tooltip("Flashing stops after this many seconds without taking damage.")]
        [SerializeField, Min(0f)] private float _stopAfter = 1f;

        private Health _health;
        private SpriteRenderer _spriteRenderer;
        private Color _baseColor;
        private float _lastHitTime = float.NegativeInfinity;
        private bool _isFlashing;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _baseColor = _spriteRenderer.color;
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            StopFlashing();
        }

        private void OnDamaged(Health health, DamageInfo damage)
        {
            _lastHitTime = Time.time;
            _isFlashing = true;
        }

        private void Update()
        {
            if (!_isFlashing)
            {
                return;
            }

            if (Time.time - _lastHitTime > _stopAfter)
            {
                StopFlashing();
                return;
            }

            // Square wave: alternate between red and the normal colour.
            bool red = Mathf.Repeat((Time.time - _lastHitTime) * _flashRate, 1f) < 0.5f;
            SetTint(red ? _flashColor : _baseColor);
        }

        private void StopFlashing()
        {
            _isFlashing = false;
            SetTint(_baseColor);
        }

        // Alpha is left alone so fades (like the boss intro) aren't undone by the flash.
        private void SetTint(Color tint)
        {
            tint.a = _spriteRenderer.color.a;
            _spriteRenderer.color = tint;
        }
    }
}
