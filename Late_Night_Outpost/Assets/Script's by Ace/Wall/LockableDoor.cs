using UnityEngine;

namespace LateNightOutpost
{
    /// <summary>
    /// Locked door that can be opened when the player:
    /// 1) Has the required key.
    /// 2) Is inside the door's use-range trigger.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class LockableDoor : MonoBehaviour
    {
        [Header("Key source")]
        [Tooltip("Component that tracks whether the player has the key.")]
        [SerializeField] private KeyPickupEffects keyPickup;

        [Header("Player filter")]
        [Tooltip("Tag used to identify the player collider in the trigger.")]
        [SerializeField] private string playerTag = "Player";

        // Cached trigger collider for range checks.
        private BoxCollider useRangeTrigger;

        // Is the player currently inside the door's use trigger?
        private bool playerInUseRange;

        private void Awake()
        {
            useRangeTrigger = GetComponent<BoxCollider>();

            if (useRangeTrigger == null)
            {
                Debug.LogError($"LockableDoor on {name}: missing BoxCollider. " +
                               "Add one and mark it as IsTrigger.");
                return;
            }

            if (!useRangeTrigger.isTrigger)
            {
                Debug.LogWarning($"LockableDoor on {name}: BoxCollider is not a trigger. " +
                                 "Set isTrigger = true for use-range detection.");
            }
        }

        /// <summary>
        /// Called by your interaction system (Activatable / PlayerInteractor)
        /// when the player presses the use key while targeting this door.
        /// </summary>
        public void TryOpenWithKey()
        {
            bool hasKey = keyPickup != null && keyPickup.HasKey;

            Debug.Log($"LockableDoor on {name}: TryOpenWithKey. " +
                      $"HasKey = {hasKey}, playerInUseRange = {playerInUseRange}");

            if (keyPickup == null)
            {
                Debug.LogError($"LockableDoor on {name}: keyPickup reference is missing.");
                return;
            }

            if (!hasKey)
            {
                Debug.Log($"LockableDoor on {name}: player tried to open without key.");
                return;
            }

            if (!playerInUseRange)
            {
                Debug.Log($"LockableDoor on {name}: player not in use range, door stays closed.");
                return;
            }

            Open();
        }

        private void Open()
        {
            // TODO: swap this for an animation / state change if you prefer
            Debug.Log($"LockableDoor on {name}: opened with key.");
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            playerInUseRange = true;
            Debug.Log($"LockableDoor on {name}: player ENTERED use range.");
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            playerInUseRange = false;
            Debug.Log($"LockableDoor on {name}: player EXITED use range.");
        }
    }
}