using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScalingShmup.UI
{
    // Handlers for the main menu buttons. Hook StartGame and Quit up to the buttons' OnClick.
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("Scene loaded once a ship is chosen. It must be listed in Build Settings.")]
        [SerializeField] private string _gameSceneName = "SampleScene";
        [Tooltip("Optional. Shown by Start Game so the player picks a ship first; without it the game starts straight away.")]
        [SerializeField] private ShipSelectWindow _shipSelectWindow;

        public void StartGame()
        {
            if (_shipSelectWindow != null)
            {
                _shipSelectWindow.Open();
            }
            else
            {
                LoadGameScene();
            }
        }

        public void LoadGameScene()
        {
            SceneManager.LoadScene(_gameSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            // Application.Quit does nothing in the Editor, so stop Play mode instead.
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
