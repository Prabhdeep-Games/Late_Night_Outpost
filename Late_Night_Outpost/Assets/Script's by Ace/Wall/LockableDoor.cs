using UnityEngine;

namespace LateNightOutpost
{
    /// <summary>
    /// Locked door that auto-opens when:
    /// 1) The player has the required key.
    /// 2) The player enters the door's trigger.
    ///
    /// No raycast or interact button required.
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

        private BoxCollider useRangeTrigger;

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

            if (keyPickup == null)
            {
                Debug.LogError($"LockableDoor on {name}: keyPickup reference is missing.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Only care about the player entering.
            if (!other.CompareTag(playerTag)) return;

            if (keyPickup == null)
            {
                Debug.LogError($"LockableDoor on {name}: keyPickup is null, cannot check key.");
                return;
            }

            bool hasKey = keyPickup.HasKey;

            Debug.Log($"LockableDoor on {name}: player ENTERED trigger. HasKey = {hasKey}");

            if (!hasKey)
            {
                // Player doesn’t have the key yet; door stays closed.
                return;
            }

            // Player has key and is in range → open immediately.
            Open();
        }

        private void Open()
        {
            Debug.Log($"LockableDoor on {name}: opened with key (auto-open on trigger).");

            // Simple behavior: disable the door. Replace with animation if needed.
            gameObject.SetActive(false);
        }
    }
}