using UnityEngine;
using UnityEngine.EventSystems;

public class UIProbe : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("UIProbe: pointer entered something.");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("UIProbe: pointer clicked something.");
    }
}