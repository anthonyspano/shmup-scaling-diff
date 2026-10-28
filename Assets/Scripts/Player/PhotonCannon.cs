using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ScalingShmup.Player
{
    // Ranged weapon. Its damage type is the only one that can strip enemy overshields.
    public class PhotonCannon : MonoBehaviour
    {
        [SerializeField] private InputActionReference _fireAction;
        [Tooltip("Bullet prefab to fire. It needs a Projectile component.")]
        [SerializeField] private GameObject _bulletPrefab;
        [Tooltip("Each muzzle fires along its local up axis.")]
        [SerializeField] private Transform[] _muzzles;
        [SerializeField, Min(0.1f)] private float _shotsPerSecond = 8f;
        [SerializeField, Min(0f)] private float _damage = 1f;
        [SerializeField] private ProjectileMotion _motion = ProjectileMotion.Straight(20f);

        private float _nextShotTime;

        private void OnEnable()
        {
            _fireAction.action.Enable();
        }

        private void Update()
        {
            if (_fireAction.action.IsPressed() && Time.time >= _nextShotTime)
            {
                Fire();
                _nextShotTime = Time.time + 1f / _shotsPerSecond;
            }
        }

        private void Fire()
        {
            ProjectilePool pool = ProjectilePool.ForPrefab(_bulletPrefab);

            if (pool == null)
            {
                return;
            }

            var damage = new DamageInfo(_damage, DamageType.PhotonCannon, gameObject);

            foreach (Transform muzzle in _muzzles)
            {
                float heading = Mathf.Atan2(muzzle.up.y, muzzle.up.x) * Mathf.Rad2Deg;
                pool.Get().Launch(muzzle.position, heading, _motion, damage);
            }
        }
    }
}
