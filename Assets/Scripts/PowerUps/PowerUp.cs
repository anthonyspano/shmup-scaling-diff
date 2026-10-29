using ScalingShmup.Audio;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Base class for power-up pickups. When the player touches it, it hands the player its effect
    // for Duration seconds and removes itself. Timed power-up types subclass this and return their
    // own effect from CreateEffect; instant ones (like Shield) override ApplyTo instead.
    // A pickup that isn't collected disappears after Lifetime seconds, blinking near the end.
    [RequireComponent(typeof(Collider2D))]
    public abstract class PowerUp : MonoBehaviour
    {
        [Tooltip("Seconds the effect lasts once picked up.")]
        [SerializeField, Min(0.1f)] private float _duration = 8f;
        [Tooltip("Played when the player picks this up.")]
        [SerializeField] private SoundCue _pickupSound = new SoundCue();

        [Header("Lifetime in the scene")]
        [Tooltip("Seconds the pickup stays in the scene before it disappears.")]
        [SerializeField, Min(0.1f)] private float _lifetime = 12f;
        [Tooltip("The pickup blinks for this many seconds before it disappears.")]
        [SerializeField, Min(0f)] private float _blinkTime = 3f;
        [Tooltip("Blinks per second while it is about to disappear.")]
        [SerializeField, Min(0.1f)] private float _blinkRate = 6f;

        private SpriteRenderer _spriteRenderer;
        private float _spawnTime;

        public float Duration => _duration;
        public float TimeLeft => _lifetime - (Time.time - _spawnTime);

        protected abstract IPowerUpEffect CreateEffect();

        // Default: start this pickup's timed effect, replacing any active one.
        protected virtual void ApplyTo(PlayerPowerUps receiver)
        {
            receiver.Activate(CreateEffect(), _duration);
        }

        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            // Pickups are touched, not bumped into.
            foreach (Collider2D pickupCollider in GetComponents<Collider2D>())
            {
                pickupCollider.isTrigger = true;
            }
        }

        protected virtual void OnEnable()
        {
            _spawnTime = Time.time;
        }

        protected virtual void Update()
        {
            float timeLeft = TimeLeft;

            if (timeLeft <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (timeLeft <= _blinkTime && _spriteRenderer != null)
            {
                // Toggle fully on and off; only alpha changes, so the animation's sprite is untouched.
                bool visible = Mathf.Repeat((_blinkTime - timeLeft) * _blinkRate, 1f) < 0.5f;
                Color color = _spriteRenderer.color;
                color.a = visible ? 1f : 0f;
                _spriteRenderer.color = color;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerPowerUps receiver = other.GetComponentInParent<PlayerPowerUps>();

            if (receiver == null)
            {
                return;
            }

            ApplyTo(receiver);
            _pickupSound.Play();
            Destroy(gameObject);
        }
    }
}
