using UnityEngine;

public class UICursorController : MonoBehaviour
{
    private void Awake()
    {
        // For UI scenes: show cursor and unlock it.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}