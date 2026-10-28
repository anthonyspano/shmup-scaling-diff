using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Base class for power-up pickups. When the player touches it, it hands the player its effect
    // for Duration seconds and removes itself. New power-up types subclass this and return their
    // own effect from CreateEffect.
    [RequireComponent(typeof(Collider2D))]
    public abstract class PowerUp : MonoBehaviour
    {
        [Tooltip("Seconds the effect lasts once picked up.")]
        [SerializeField, Min(0.1f)] private float _duration = 8f;

        public float Duration => _duration;

        protected abstract IPowerUpEffect CreateEffect();

        protected virtual void Awake()
        {
            // Pickups are touched, not bumped into.
            foreach (Collider2D pickupCollider in GetComponents<Collider2D>())
            {
                pickupCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerPowerUps receiver = other.GetComponentInParent<PlayerPowerUps>();

            if (receiver == null)
            {
                return;
            }

            receiver.Activate(CreateEffect(), _duration);
            Destroy(gameObject);
        }
    }
}
