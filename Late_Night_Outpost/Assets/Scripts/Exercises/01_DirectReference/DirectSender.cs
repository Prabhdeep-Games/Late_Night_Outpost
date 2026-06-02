using UnityEngine;

public class DirectSender : MonoBehaviour
{
    // Option A: drag the receiver in from the inspector.
    [SerializeField] private DirectReceiver receiver;

    // Option B: grab a sibling component on the same GameObject.
    // private void Awake() => receiver = GetComponent<DirectReceiver>();

    // Option C: search the whole scene at runtime (slow — avoid in Update).
    // private void Awake() => receiver = FindFirstObjectByType<DirectReceiver>();

    [ContextMenu("Increment Counter")]
    private void IncrementCounter() => receiver.counter++;

    [ContextMenu("Set Message")]
    private void SetMessage() => receiver.message = "Hello from DirectSender";
}
