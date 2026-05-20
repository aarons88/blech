using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blech.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] public string verticalSliceSceneName = "MVP_VerticalSlice";

        public void Play() => SceneManager.LoadScene(verticalSliceSceneName);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
