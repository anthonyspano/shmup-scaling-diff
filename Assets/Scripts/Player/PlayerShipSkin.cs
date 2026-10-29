using UnityEngine;

namespace ScalingShmup.Player
{
    // Gives the player ship the sprite chosen on the main menu. With no choice made (e.g. when the
    // game scene is played directly in the Editor), the ship keeps the sprite it already has.
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerShipSkin : MonoBehaviour
    {
        private void Awake()
        {
            if (PlayerShipChoice.Selected != null)
            {
                GetComponent<SpriteRenderer>().sprite = PlayerShipChoice.Selected;
            }
        }
    }
}
