using ScalingShmup.Player;
using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // Swaps the photon cannon to firing firework shells, then restores its normal bullet.
    public class FireworkEffect : IPowerUpEffect
    {
        private readonly GameObject _shellPrefab;
        private readonly float _shotsPerSecond;
        private readonly float _burstDistance;
        private PhotonCannon _cannon;

        public FireworkEffect(GameObject shellPrefab, float shotsPerSecond, float burstDistance)
        {
            _shellPrefab = shellPrefab;
            _shotsPerSecond = shotsPerSecond;
            _burstDistance = burstDistance;
        }

        public string Name => "Firework";

        public void Apply(GameObject player)
        {
            if (player.TryGetComponent(out _cannon))
            {
                _cannon.SetBullet(_shellPrefab, _shotsPerSecond);
                _cannon.Fired += SetShellRange;
            }
        }

        public void Remove(GameObject player)
        {
            if (_cannon != null)
            {
                _cannon.Fired -= SetShellRange;
                _cannon.ResetBullet();
            }
        }

        private void SetShellRange(Projectile bullet)
        {
            if (bullet.TryGetComponent(out FireworkShell shell))
            {
                shell.Range = _burstDistance;
            }
        }
    }
}
