using UnityEngine;

public class SingletonSender : MonoBehaviour
{
    [ContextMenu("Increment Counter")]
    private void IncrementCounter() => SingletonReceiver.Instance.counter++;

    [ContextMenu("Set Message")]
    private void SetMessage() => SingletonReceiver.Instance.message = "Hello from SingletonSender";
}
