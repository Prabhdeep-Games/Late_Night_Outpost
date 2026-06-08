using UnityEngine;

public class StaticReceiver : MonoBehaviour
{
    // Static = lives on the class, not on any instance. Shared by everyone.
    public static int counter;
    public static string message;

    // Statics don't appear in the inspector — mirror them so we can see them in Play mode.
    [SerializeField] private int counterMirror;
    [SerializeField] private string messageMirror;

    private void Update()
    {
        counterMirror = counter;
        messageMirror = message;
    }
}
