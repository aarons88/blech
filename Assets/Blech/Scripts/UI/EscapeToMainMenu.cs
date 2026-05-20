using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Blech.UI
{
    public class EscapeToMainMenu : MonoBehaviour
    {
        [SerializeField] public string mainMenuSceneName = "MainMenu";

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
