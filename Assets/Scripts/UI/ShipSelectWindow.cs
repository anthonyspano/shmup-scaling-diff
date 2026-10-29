using ScalingShmup.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScalingShmup.UI
{
    // Window listing the ship colours in a row. Clicking one stores it as the player's ship and
    // asks the main menu to start the game. One button is built per sprite in Ship Options the
    // first time the window opens. Keep the window switched off in the scene; Open shows it.
    public class ShipSelectWindow : MonoBehaviour
    {
        [Tooltip("Ship sprites to choose from, shown left to right.")]
        [SerializeField] private Sprite[] _shipOptions;
        [Tooltip("Row the buttons are placed in (with a Horizontal Layout Group).")]
        [SerializeField] private RectTransform _buttonRow;
        [Tooltip("Button copied once per ship. It needs an Image child named \"Ship\" for the sprite.")]
        [SerializeField] private Button _buttonTemplate;
        [SerializeField] private MainMenu _mainMenu;

        private Button _firstButton;

        private void BuildButtons()
        {
            _buttonTemplate.gameObject.SetActive(false);

            foreach (Sprite ship in _shipOptions)
            {
                Button button = Instantiate(_buttonTemplate, _buttonRow);
                button.name = $"Ship {ship.name}";
                button.gameObject.SetActive(true);

                Image icon = button.transform.Find("Ship").GetComponent<Image>();
                icon.sprite = ship;
                icon.preserveAspect = true;

                Sprite chosen = ship;
                button.onClick.AddListener(() => Choose(chosen));

                if (_firstButton == null)
                {
                    _firstButton = button;
                }
            }
        }

        public void Open()
        {
            if (_firstButton == null)
            {
                BuildButtons();
            }

            gameObject.SetActive(true);

            // Keyboard and gamepad users start on the first ship.
            if (_firstButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_firstButton.gameObject);
            }
        }

        private void Choose(Sprite ship)
        {
            PlayerShipChoice.Select(ship);
            _mainMenu.LoadGameScene();
        }
    }
}
