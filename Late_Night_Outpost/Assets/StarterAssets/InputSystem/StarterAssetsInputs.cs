using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        // Called by PlayerInput (Send Messages) when the Move action changes.
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        // Called by PlayerInput when the Look action changes.
        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        // Called by PlayerInput when the Jump action changes.
        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        // Called by PlayerInput when the Sprint action changes.
        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        // NEW: Called by PlayerInput when the Pause action (Esc) is triggered.
        public void OnPause(InputValue value)
        {
            if (!value.isPressed) return;

            Debug.Log("StarterAssetsInputs: Pause action triggered, toggling pause.");

            // Find PauseSystem in the scene and toggle pause.
            var pauseSystem = FindObjectOfType<PauseSystem>();
            if (pauseSystem != null)
            {
                pauseSystem.TogglePause();
            }
            else
            {
                Debug.LogWarning("StarterAssetsInputs: No PauseSystem found in scene.");
            }
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}