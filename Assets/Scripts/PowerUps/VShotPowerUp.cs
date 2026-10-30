using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // "V Shot" pickup: for Duration seconds the player's cannon fires straight ahead and at +45
    // and -45 degrees from the ship's forward direction.
    public class VShotPowerUp : PowerUp
    {
        [Tooltip("Shot angles in degrees from the ship's forward direction while V Shot is active.")]
        [SerializeField] private float[] _shotAngles = { 45f, 0f, -45f };

        protected override IPowerUpEffect CreateEffect()
        {
            return new VShotEffect(_shotAngles);
        }
    }
}
