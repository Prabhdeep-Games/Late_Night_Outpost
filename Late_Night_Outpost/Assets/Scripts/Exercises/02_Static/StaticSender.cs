using UnityEngine;

public class StaticSender : MonoBehaviour
{
    // No reference needed — talk to the class directly.
    [ContextMenu("Increment Counter")]
    private void IncrementCounter() => StaticReceiver.counter++;

    [ContextMenu("Set Message")]
    private void SetMessage() => StaticReceiver.message = "Hello from StaticSender";
}
