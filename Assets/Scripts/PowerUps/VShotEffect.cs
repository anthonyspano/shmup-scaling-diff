using ScalingShmup.Player;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Swaps the photon cannon to a V-shaped pattern, then restores the cannon's default pattern.
    public class VShotEffect : IPowerUpEffect
    {
        private readonly float[] _shotAngles;

        public VShotEffect(float[] shotAngles)
        {
            _shotAngles = shotAngles;
        }

        public string Name => "V Shot";

        public void Apply(GameObject player)
        {
            if (player.TryGetComponent(out PhotonCannon cannon))
            {
                cannon.SetShotAngles(_shotAngles);
            }
        }

        public void Remove(GameObject player)
        {
            if (player.TryGetComponent(out PhotonCannon cannon))
            {
                cannon.ResetShotPattern();
            }
        }
    }
}
