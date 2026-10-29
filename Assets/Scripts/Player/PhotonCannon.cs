using ScalingShmup.Combat;
using ScalingShmup.Projectiles;
using ScalingShmup.Audio;
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
        [Tooltip("The normal shot pattern: one bullet per angle, in degrees from the ship's forward direction.")]
        [SerializeField] private float[] _defaultShotAngles = { 0f };
        [Tooltip("Played once per shot (not per bullet).")]
        [SerializeField] private SoundCue _fireSound = new SoundCue();

        private float[] _shotAngles;
        private GameObject _activeBulletPrefab;
        private float _activeShotsPerSecond;
        private float _nextShotTime;

        // Raised for every bullet fired, so power-ups can adjust their own bullets (e.g. firework range).
        public event System.Action<Projectile> Fired;

        private void Awake()
        {
            _shotAngles = _defaultShotAngles;
            _activeBulletPrefab = _bulletPrefab;
            _activeShotsPerSecond = _shotsPerSecond;
        }

        // Power-ups swap the bullet (and its fire rate) temporarily, then call ResetBullet.
        public void SetBullet(GameObject bulletPrefab, float shotsPerSecond)
        {
            _activeBulletPrefab = bulletPrefab != null ? bulletPrefab : _bulletPrefab;
            _activeShotsPerSecond = shotsPerSecond > 0f ? shotsPerSecond : _shotsPerSecond;
        }

        public void ResetBullet()
        {
            _activeBulletPrefab = _bulletPrefab;
            _activeShotsPerSecond = _shotsPerSecond;
        }

        // Power-ups swap the shot pattern temporarily, then call ResetShotPattern.
        public void SetShotAngles(float[] angles)
        {
            _shotAngles = angles != null && angles.Length > 0 ? angles : _defaultShotAngles;
        }

        public void ResetShotPattern()
        {
            _shotAngles = _defaultShotAngles;
        }

        private void OnEnable()
        {
            _fireAction.action.Enable();
        }

        private void Update()
        {
            if (_fireAction.action.IsPressed() && Time.time >= _nextShotTime)
            {
                Fire();
                _nextShotTime = Time.time + 1f / _activeShotsPerSecond;
            }
        }

        private void Fire()
        {
            ProjectilePool pool = ProjectilePool.ForPrefab(_activeBulletPrefab);

            if (pool == null)
            {
                return;
            }

            var damage = new DamageInfo(_damage, DamageType.PhotonCannon, gameObject);
            _fireSound.Play();

            foreach (Transform muzzle in _muzzles)
            {
                float forward = Mathf.Atan2(muzzle.up.y, muzzle.up.x) * Mathf.Rad2Deg;

                foreach (float angle in _shotAngles)
                {
                    Projectile bullet = pool.Get();
                    bullet.Launch(muzzle.position, forward + angle, _motion, damage);
                    Fired?.Invoke(bullet);
                }
            }
        }
    }
}
