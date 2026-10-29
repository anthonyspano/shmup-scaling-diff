using ScalingShmup.Combat;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // "Shield" pickup: instantly restores shield points to the player, up to their maximum. It is
    // not a timed power-up, so it leaves any active power-up (V Shot, Firework) running.
    public class ShieldPowerUp : PowerUp
    {
        [Tooltip("Shield points restored on pickup.")]
        [SerializeField, Min(0f)] private float _shieldRestored = 1f;

        // Never used: ApplyTo is overridden so no timed effect is started.
        protected override IPowerUpEffect CreateEffect()
        {
            return null;
        }

        protected override void ApplyTo(PlayerPowerUps receiver)
        {
            if (receiver.TryGetComponent(out Health health))
            {
                health.AddOvershield(_shieldRestored);
            }
        }
    }
}
