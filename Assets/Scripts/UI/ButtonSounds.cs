using ScalingShmup.Audio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ScalingShmup.UI
{
    // Plays a sound when the mouse moves over this button and another when it is clicked (or
    // pressed with a keyboard or gamepad). Put it on the button; buttons copied from a template
    // (like the ship buttons) get the template's sounds.
    [RequireComponent(typeof(Button))]
    public class ButtonSounds : MonoBehaviour, IPointerEnterHandler
    {
        [Tooltip("Played when the mouse moves over the button.")]
        [SerializeField] private SoundCue _hoverSound = new SoundCue();
        [Tooltip("Played when the button is clicked.")]
        [SerializeField] private SoundCue _clickSound = new SoundCue();

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(_clickSound.Play);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(_clickSound.Play);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_button.IsInteractable())
            {
                _hoverSound.Play();
            }
        }
    }
}
