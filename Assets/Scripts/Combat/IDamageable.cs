namespace ScalingShmup.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }

        // Returns true when any of the damage was applied.
        bool TakeDamage(DamageInfo damage);
    }
}
