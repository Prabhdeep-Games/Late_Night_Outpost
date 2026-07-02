using UnityEngine;
using UnityEngine.UI;

public class KeyPickupEffects : MonoBehaviour
{
    [Header("UI feedback")]
    [Tooltip("UI Image that shows when the key is collected.")]
    [SerializeField] private Image keyIcon;

    [Header("Key object")]
    [Tooltip("The key GameObject in the world to hide after pickup.")]
    [SerializeField] private GameObject keyObject;

    [Header("Player filter")]
    [Tooltip("Only objects with this tag can collect the key.")]
    [SerializeField] private string playerTag = "Player";

    // Internal state: does the player currently have the key?
    private bool hasKey;

    // Public read-only property so doors can check if the player has the key.
    public bool HasKey => hasKey;

    // Trigger pickup: when player enters the key's trigger, grant the key.
    private void OnTriggerEnter(Collider other)
    {
        // Already collected? Do nothing.
        if (hasKey) return;

        // Only allow the player to collect the key.
        if (!other.CompareTag(playerTag)) return;

        CollectKey();
    }

    // Core pickup logic: grant key, show UI, hide key object.
    private void CollectKey()
    {
        hasKey = true;

        // Show UI feedback.
        if (keyIcon)
        {
            keyIcon.gameObject.SetActive(true);
            keyIcon.enabled = true;
        }

        // Hide the physical key object in the world.
        if (keyObject)
        {
            keyObject.SetActive(false);
        }
    }

    // Optional: if someday you want to remove the key from the player.
    public void RemoveKey()
    {
        hasKey = false;

        if (keyIcon)
        {
            keyIcon.gameObject.SetActive(false);
        }
    }
}