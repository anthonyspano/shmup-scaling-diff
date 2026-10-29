using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace ScalingShmup.Player
{
    // Carries the ship sprite picked on the main menu into the game scene. AutoStaticsCleanup
    // clears it when Play mode starts, so the Editor doesn't keep an old choice.
    [AutoStaticsCleanup]
    public static partial class PlayerShipChoice
    {
        public static Sprite Selected { get; private set; }

        public static void Select(Sprite ship)
        {
            Selected = ship;
        }
    }
}
