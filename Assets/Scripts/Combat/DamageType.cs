using System;

namespace ScalingShmup.Combat
{
    [Flags]
    public enum DamageType
    {
        None = 0,
        PhotonCannon = 1 << 0,
        Melee = 1 << 1,
        EnemyFire = 1 << 2,
        Contact = 1 << 3,
    }
}
