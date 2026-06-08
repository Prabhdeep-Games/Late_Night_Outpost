using System;
using UnityEngine;

public class ActionSender : MonoBehaviour
{
    // C# event. Anyone can subscribe; the sender doesn't know or care who listens.
    public static event Action<int> OnCounterChanged;
    public static event Action<string> OnMessageChanged;

    private int counter;

    [ContextMenu("Increment Counter")]
    private void IncrementCounter()
    {
        counter++;
        OnCounterChanged?.Invoke(counter);
    }

    [ContextMenu("Set Message")]
    private void SetMessage() => OnMessageChanged?.Invoke("Hello from ActionSender");
}
