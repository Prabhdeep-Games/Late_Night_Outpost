using UnityEngine;
using UnityEngine.UI;

public class KeyPickupEffects : MonoBehaviour
{
    [Header("Doors to remove")]
    [SerializeField] private GameObject[] doors;

    [Header("UI")]
    [SerializeField] private Image keyIcon;
    [SerializeField] private Text keyText; // optional, for "Key" label

    [Header("Key object")]
    [SerializeField] private GameObject keyObject;

    [Header("Player filter")]
    [SerializeField] private string playerTag = "Player";

    private bool collected;

    // Option A: called from Activatable.onActivate (if you ever use interact)
    public void OnKeyUsed()
    {
        if (collected) return;
        collected = true;

        RemoveDoors();
        ShowKeyUI();
        HideKeyObject();
    }

    // Option B: trigger-based pickup (walk into the key)
    private void OnTriggerEnter(Collider other)
{
    Debug.Log($"Key trigger hit by: {other.name}");

    if (collected) return;
    if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
        return;

    OnKeyUsed();
}

    private void RemoveDoors()
{
    for (int i = 0; i < doors.Length; i++)
    {
        if (doors[i])
        {
            // Instead of Destroy(doors[i]);
            doors[i].SetActive(false);
        }
    }
}

    private void ShowKeyUI()
    {
        if (keyIcon) keyIcon.enabled = true;
        if (keyText) keyText.text = "Key"; // or "Key collected"
    }

    private void HideKeyObject()
    {
        if (keyObject)
        {
            keyObject.SetActive(false);
        }
    }

    
}