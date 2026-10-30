using UnityEngine;
using UnityEngine.InputSystem;

namespace ScalingShmup.UI
{
    // Closes the game when Escape is pressed. Application.Quit does nothing in the Editor, so
    // there it stops Play mode instead.
    public class QuitOnEscape : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
