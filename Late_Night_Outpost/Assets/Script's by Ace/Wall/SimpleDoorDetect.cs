using UnityEngine;

public class SimpleLockableDoor : MonoBehaviour
{
    [Header("Key source")]
    [SerializeField] private KeyPickupEffects keyPickup;

    [Header("Player filter")]
    [SerializeField] private string playerTag = "Player";

    // Player in trigger?
    private bool playerInUseRange;

    private void Awake()
    {
        var trigger = GetComponent<BoxCollider>();
        if (trigger == null)
        {
            Debug.LogWarning($"SimpleLockableDoor on {name}: needs a BoxCollider trigger.");
        }
        else if (!trigger.isTrigger)
        {
            Debug.LogWarning($"SimpleLockableDoor on {name}: BoxCollider must be set as IsTrigger.");
        }
    }

    private void Update()
    {
        // Press E to try opening when in range.
        if (playerInUseRange && Input.GetKeyDown(KeyCode.E))
        {
            TryOpenWithKey();
        }
    }

    private void TryOpenWithKey()
    {
        Debug.Log($"SimpleLockableDoor on {name}: TryOpenWithKey. " +
                  $"HasKey = {(keyPickup ? keyPickup.HasKey : false)}, " +
                  $"playerInUseRange = {playerInUseRange}");

        if (keyPickup == null)
        {
            Debug.LogWarning($"SimpleLockableDoor on {name}: keyPickup not assigned.");
            return;
        }

        if (!keyPickup.HasKey)
        {
            Debug.Log($"SimpleLockableDoor on {name}: player tried to open without key.");
            return;
        }

        if (!playerInUseRange)
        {
            Debug.Log($"SimpleLockableDoor on {name}: player NOT in use range.");
            return;
        }

        Debug.Log($"SimpleLockableDoor on {name}: opened with key.");
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInUseRange = true;
            Debug.Log($"SimpleLockableDoor on {name}: player ENTERED use range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInUseRange = false;
            Debug.Log($"SimpleLockableDoor on {name}: player EXITED use range.");
        }
    }
}