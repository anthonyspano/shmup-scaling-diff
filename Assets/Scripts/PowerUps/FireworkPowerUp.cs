using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // "Firework" pickup: for Duration seconds the player's cannon fires firework shells, which
    // fly Burst Distance and then burst into fragments. Fragment settings live on the shell prefab's
    // FireworkShell component.
    public class FireworkPowerUp : PowerUp
    {
        [Tooltip("Bullet prefab fired while Firework is active. It needs Projectile and FireworkShell components.")]
        [SerializeField] private GameObject _fireworkShellPrefab;
        [Tooltip("Cannon fire rate while Firework is active. Bursts are big, so this is lower than normal.")]
        [SerializeField, Min(0.1f)] private float _shotsPerSecond = 2f;
        [Tooltip("Distance in units each firework shell flies before it explodes.")]
        [SerializeField, Min(0.1f)] private float _burstDistance = 6f;

        protected override IPowerUpEffect CreateEffect()
        {
            return new FireworkEffect(_fireworkShellPrefab, _shotsPerSecond, _burstDistance);
        }
    }
}
