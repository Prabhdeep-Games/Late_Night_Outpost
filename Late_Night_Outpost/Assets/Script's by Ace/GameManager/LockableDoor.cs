using UnityEngine;

public class LockableDoor : MonoBehaviour
{
    [Header("Key source")]
    [SerializeField] private KeyPickupEffects keyPickup;

    [Header("Player filter")]
    [SerializeField] private string playerTag = "Player";

    // We don’t expose useRangeTrigger anymore; we’ll grab it automatically.
    private BoxCollider useRangeTrigger;

    // Is the player currently inside the door's use trigger?
    private bool playerInUseRange;

    private void Awake()
    {
        // Try to find a BoxCollider on this GameObject.
        useRangeTrigger = GetComponent<BoxCollider>();

        if (useRangeTrigger == null)
        {
            Debug.LogWarning($"LockableDoor on {name}: no BoxCollider found. Add one with IsTrigger = true.");
        }
        else if (!useRangeTrigger.isTrigger)
        {
            Debug.LogWarning($"LockableDoor on {name}: BoxCollider is not marked as IsTrigger. Set isTrigger = true.");
        }
    }

    // Called by Activatable when E is pressed while looking at this door.
    public void TryOpenWithKey()
    {
        if (keyPickup == null)
        {
            Debug.LogWarning($"LockableDoor on {name}: keyPickup is not assigned.");
            return;
        }

        if (!keyPickup.HasKey)
        {
            Debug.Log($"LockableDoor on {name}: player tried to open without key.");
            return;
        }

        if (!playerInUseRange)
        {
            Debug.Log($"LockableDoor on {name}: player not in use range.");
            return;
        }

        Debug.Log($"LockableDoor on {name}: opened with key.");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (useRangeTrigger == null) return;
        if (!other.CompareTag(playerTag)) return;

        playerInUseRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (useRangeTrigger == null) return;
        if (!other.CompareTag(playerTag)) return;

        playerInUseRange = false;
    }
}