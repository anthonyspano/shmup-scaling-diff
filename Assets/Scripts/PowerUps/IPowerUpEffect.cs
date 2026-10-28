using UnityEngine;

namespace ScalingShmup.PowerUps
{
    // What a power-up does to the player while it is active. Remove must undo Apply exactly.
    public interface IPowerUpEffect
    {
        string Name { get; }
        void Apply(GameObject player);
        void Remove(GameObject player);
    }
}
