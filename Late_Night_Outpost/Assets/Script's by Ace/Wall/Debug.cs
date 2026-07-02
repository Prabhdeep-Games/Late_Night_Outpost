using UnityEngine;

public class DoorDebugInteract : MonoBehaviour
{
    public void OnDoorInteracted()
    {
        Debug.Log($"DoorDebugInteract: E pressed on {name}");
    }
}