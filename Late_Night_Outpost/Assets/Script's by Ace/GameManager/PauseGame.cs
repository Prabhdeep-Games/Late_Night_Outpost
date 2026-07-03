using UnityEngine;
using UnityEngine.InputSystem;

public class PauseSystem : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    private bool isPaused;

    private void Awake()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        Time.timeScale = isPaused ? 0f : 1f;

        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Resume()
    {
        if (!isPaused) return;
        TogglePause();
    }
}