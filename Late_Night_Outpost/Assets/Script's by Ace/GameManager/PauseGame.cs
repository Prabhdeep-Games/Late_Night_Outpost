using UnityEngine;
using UnityEngine.InputSystem;

public class PauseSystem : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("UI panel shown when the game is paused.")]
    [SerializeField] private GameObject pauseMenu;

    private bool isPaused;

    private void Awake()
    {
        // Ensure game starts unpaused.
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseMenu != null)
            pauseMenu.SetActive(false);
    }

    /// <summary>
    /// Called by PlayerInput when the 'Pause' action (Escape key) is triggered.
    /// Wire this under PlayerInput.Events → Pause.
    /// </summary>
    public void OnPause(InputAction.CallbackContext context)
    {
        // Debug: see every phase.
        Debug.Log($"PauseSystem: Pause action phase = {context.phase}");

        // Only toggle on the actual button press.
        if (!context.performed) return;

        Debug.Log("PauseSystem: Pause action PERFORMED, toggling pause.");
        TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        // Freeze or resume gameplay.
        Time.timeScale = isPaused ? 0f : 1f;

        // Show/hide the pause UI.
        if (pauseMenu != null)
            pauseMenu.SetActive(isPaused);

        // Cursor behavior (optional, good for 3rd-person / FPS).
        Cursor.visible = isPaused;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    /// <summary>
    /// Hook this to a Resume button in the pause menu.
    /// </summary>
    public void Resume()
    {
        if (!isPaused) return;
        TogglePause();
    }
}