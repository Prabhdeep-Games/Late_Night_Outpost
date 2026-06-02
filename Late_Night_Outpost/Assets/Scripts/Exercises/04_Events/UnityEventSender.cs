using UnityEngine;
using UnityEngine.Events;

public class UnityEventSender : MonoBehaviour
{
    // UnityEvents are wired in the inspector — same as a Button's OnClick.
    public UnityEvent<int> onCounterChanged;
    public UnityEvent<string> onMessageChanged;

    private int counter;
    
    //void Update() => counter++; Not doing this here - because we're using UnityEvents in inspector

    [ContextMenu("Increment Counter")]
    private void IncrementCounter()
    {
        counter++;
        onCounterChanged.Invoke(counter);
    }

    [ContextMenu("Set Message")]
    private void SetMessage() => onMessageChanged.Invoke("Hello from UnityEventSender");
}
