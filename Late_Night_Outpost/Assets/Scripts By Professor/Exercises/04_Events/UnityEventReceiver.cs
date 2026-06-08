using UnityEngine;

public class UnityEventReceiver : MonoBehaviour
{
    public int counter;
    public string message;

    // Wire these from the UnityEventSender's inspector. No code subscription needed.
    public void SetCounter(int value) => counter = value;
    public void SetMessage(string value) => message = value;
}
