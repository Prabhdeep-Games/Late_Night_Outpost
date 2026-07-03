using UnityEngine;

public class ExitGame : MonoBehaviour
{
    // Call this from a UI Button's OnClick.
    public void QuitGame()
    {
        Debug.Log("ExitGame: quitting application.");

        // In a built game, this closes the application.
        Application.Quit();

        // In the editor, Application.Quit does nothing,
        // so this line is just to confirm the call in logs.
#if UNITY_EDITOR
        Debug.Log("ExitGame: Application.Quit() called (Editor only, game will not actually close).");
#endif
    }
}