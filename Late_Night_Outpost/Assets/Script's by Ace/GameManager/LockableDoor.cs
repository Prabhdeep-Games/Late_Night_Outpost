using UnityEngine;

public class LockableDoor : MonoBehaviour
{
    [Header("Key source")]
    [SerializeField] private KeyPickupEffects keyPickup;

    // This MUST be public, return void, and take NO parameters
    public void TryOpenWithKey()
    {
        if (keyPickup == null)
        {
            Debug.LogWarning($"Door {name}: no KeyPickupEffects assigned.");
            return;
        }

        if (!keyPickup.HasKey)
        {
            Debug.Log($"Door {name}: player tried to open without key.");
            return;
        }

        Debug.Log($"Door {name}: opened with key.");
        gameObject.SetActive(false);

        keyPickup.ConsumeKey();
    }

    // Gizmos optional
    private void OnDrawGizmos()
    {
        // ...
    }
}