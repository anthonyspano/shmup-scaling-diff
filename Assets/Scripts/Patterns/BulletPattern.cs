using ScalingShmup.Projectiles;
using UnityEngine;

namespace ScalingShmup.Patterns
{
    public enum PatternAim
    {
        FixedAngle,
        AtTarget,
    }

    // Parametric description of a bullet pattern. The heading of bullet i in burst b, fired t
    // seconds after the pattern starts, is:
    //   aim + spread(i) + b * SpinPerBurst + t * SpinSpeed + SweepAmplitude * sin(2 pi SweepFrequency t)
    // and its launch speed is Motion.Speed + b * SpeedChangePerBurst.
    // Rings, fans, spirals, sweeps and aimed bursts are all combinations of these parameters.
    [CreateAssetMenu(menuName = "Scaling Shmup/Bullet Pattern", fileName = "BulletPattern")]
    public class BulletPattern : ScriptableObject
    {
        [Header("Aim")]
        [SerializeField] private PatternAim _aim = PatternAim.FixedAngle;
        [Tooltip("Degrees, used by FixedAngle. 0 is +X, -90 is straight down the screen.")]
        [SerializeField] private float _baseAngle = -90f;
        [Tooltip("Degrees added to the target direction, used by AtTarget.")]
        [SerializeField] private float _aimOffset = 0f;

        [Header("Timing")]
        [SerializeField, Min(1)] private int _bursts = 1;
        [SerializeField, Min(0f)] private float _burstInterval = 0.2f;
        [Tooltip("Pause after the last burst before the emitter moves to the next pattern.")]
        [SerializeField, Min(0f)] private float _delayAfter = 0.5f;

        [Header("Shape")]
        [SerializeField, Min(1)] private int _bulletsPerBurst = 1;
        [Tooltip("Degrees covered by one burst. 360 or more gives an evenly spaced ring.")]
        [SerializeField, Range(0f, 360f)] private float _spreadAngle = 0f;

        [Header("Rotation over time")]
        [SerializeField] private float _spinPerBurst = 0f;
        [Tooltip("Degrees per second.")]
        [SerializeField] private float _spinSpeed = 0f;
        [SerializeField] private float _sweepAmplitude = 0f;
        [SerializeField, Min(0f)] private float _sweepFrequency = 0f;

        [Header("Bullets")]
        [SerializeField] private ProjectileMotion _motion = ProjectileMotion.Straight(5f);
        [SerializeField] private float _speedChangePerBurst = 0f;
        [SerializeField, Min(0f)] private float _damage = 1f;

        public PatternAim Aim => _aim;
        public int Bursts => _bursts;
        public float BurstInterval => _burstInterval;
        public float DelayAfter => _delayAfter;
        public int BulletsPerBurst => _bulletsPerBurst;
        public float Damage => _damage;

        public float GetHeading(int burst, int bullet, float time, float aimAngle)
        {
            float aim = _aim == PatternAim.AtTarget ? aimAngle + _aimOffset : _baseAngle;
            float sweep = _sweepAmplitude * Mathf.Sin(2f * Mathf.PI * _sweepFrequency * time);
            return aim + GetSpreadOffset(bullet) + burst * _spinPerBurst + time * _spinSpeed + sweep;
        }

        public ProjectileMotion GetMotion(int burst)
        {
            ProjectileMotion motion = _motion;
            motion.Speed += burst * _speedChangePerBurst;
            return motion;
        }

        private float GetSpreadOffset(int bullet)
        {
            if (_bulletsPerBurst <= 1)
            {
                return 0f;
            }

            // A full ring would put the first and last bullets on top of each other.
            if (_spreadAngle >= 360f)
            {
                return bullet * 360f / _bulletsPerBurst;
            }

            return -_spreadAngle * 0.5f + bullet * _spreadAngle / (_bulletsPerBurst - 1);
        }
    }
}
