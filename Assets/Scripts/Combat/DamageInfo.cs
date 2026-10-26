using UnityEngine;

namespace ScalingShmup.Combat
{
    public readonly struct DamageInfo
    {
        public float Amount { get; }
        public DamageType Type { get; }
        public GameObject Source { get; }

        public DamageInfo(float amount, DamageType type, GameObject source)
        {
            Amount = amount;
            Type = type;
            Source = source;
        }
    }
}
