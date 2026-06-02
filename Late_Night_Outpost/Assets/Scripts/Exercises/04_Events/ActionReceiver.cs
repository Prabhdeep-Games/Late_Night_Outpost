using UnityEngine;

public class ActionReceiver : MonoBehaviour
{
    public int counter;
    public string message;

    // Subscribe in OnEnable, unsubscribe in OnDisable — always pair them.
    private void OnEnable()
    {
        ActionSender.OnCounterChanged += HandleCounter;
        ActionSender.OnMessageChanged += HandleMessage;
    }

    private void OnDisable()
    {
        ActionSender.OnCounterChanged -= HandleCounter;
        ActionSender.OnMessageChanged -= HandleMessage;
    }

    private void HandleCounter(int value) => counter = value;
    private void HandleMessage(string value) => message = value;
}
