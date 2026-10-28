using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // "V Shot" pickup: for Duration seconds the player's cannon fires at +45 and -45 degrees from
    // the ship's forward direction instead of straight ahead.
    public class VShotPowerUp : PowerUp
    {
        [Tooltip("Shot angles in degrees from the ship's forward direction while V Shot is active.")]
        [SerializeField] private float[] _shotAngles = { 45f, -45f };

        protected override IPowerUpEffect CreateEffect()
        {
            return new VShotEffect(_shotAngles);
        }
    }
}
