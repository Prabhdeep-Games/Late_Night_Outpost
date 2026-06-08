using UnityEngine;

public class SharedDataSender : MonoBehaviour
{
    // Drag the SharedData asset here.
    [SerializeField] private SharedData data;

    // Writes go through the properties so Receiver's OnChanged subscribers fire.
    [ContextMenu("Increment Counter")]
    private void IncrementCounter() => data.Counter++;

    [ContextMenu("Set Message")]
    private void SetMessage() => data.Message = "Hello from SharedDataSender";
}
