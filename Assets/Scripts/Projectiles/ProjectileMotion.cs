using System;
using UnityEngine;

namespace ScalingShmup.Projectiles
{
    // Parametric description of how a single projectile travels after it is fired.
    [Serializable]
    public struct ProjectileMotion
    {
        [Tooltip("Units per second at launch.")]
        public float Speed;

        [Tooltip("Change in speed per second. Negative values slow the projectile down.")]
        public float Acceleration;

        [Tooltip("Speed is clamped to this range so decelerating bullets stop instead of reversing.")]
        public Vector2 SpeedRange;

        [Tooltip("Degrees per second the heading turns. Produces curving and spiralling bullets.")]
        public float AngularVelocity;

        [Tooltip("Sideways sine-wave offset in units.")]
        public float WaveAmplitude;

        [Tooltip("Sine-wave cycles per second.")]
        public float WaveFrequency;

        public static ProjectileMotion Straight(float speed)
        {
            return new ProjectileMotion
            {
                Speed = speed,
                SpeedRange = new Vector2(0f, speed),
            };
        }
    }
}
