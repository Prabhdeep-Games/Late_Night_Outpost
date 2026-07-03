using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Name of the scene to load when the button is clicked.")]
    [SerializeField] private string sceneName;

    // Called from the UI Button's OnClick.
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneLoader: sceneName is empty. Set it in the Inspector.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}