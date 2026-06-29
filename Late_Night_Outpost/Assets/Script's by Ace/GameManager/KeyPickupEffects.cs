using UnityEngine;
using UnityEngine.UI;

public class KeyPickupEffects : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image keyIcon;
    [SerializeField] private Text keyText;

    [Header("Key object")]
    [SerializeField] private GameObject keyObject;

    [Header("Player filter")]
    [SerializeField] private string playerTag = "Player";

    private bool collected;

    public bool HasKey => collected;

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        CollectKey();
    }

    private void CollectKey()
    {
        if (collected) return;
        collected = true;

        ShowKeyUI();
        HideKeyObject();
    }

    private void ShowKeyUI()
    {
        if (keyIcon)
        {
            keyIcon.gameObject.SetActive(true);
            keyIcon.enabled = true;
        }

        if (keyText)
        {
            keyText.gameObject.SetActive(true);
            keyText.text = "Key"; // or "Press E near a door"
        }
    }

    private void HideKeyObject()
    {
        if (keyObject)
        {
            keyObject.SetActive(false);
        }
    }

    public void ConsumeKey()
    {
        collected = false;

        if (keyIcon) keyIcon.gameObject.SetActive(false);
        if (keyText) keyText.gameObject.SetActive(false);
    }
}