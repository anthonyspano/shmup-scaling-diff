using UnityEngine;
using UnityEngine.SceneManagement;

namespace ScalingShmup.UI
{
    // Handlers for the main menu buttons. Hook StartGame and Quit up to the buttons' OnClick.
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("Scene loaded by Start Game. It must be listed in Build Settings.")]
        [SerializeField] private string _gameSceneName = "SampleScene";

        public void StartGame()
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
