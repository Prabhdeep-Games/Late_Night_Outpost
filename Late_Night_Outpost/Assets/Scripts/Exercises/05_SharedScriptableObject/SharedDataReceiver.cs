using UnityEngine;

public class SharedDataReceiver : MonoBehaviour
{
    // Drag the same SharedData asset here.
    [SerializeField] private SharedData data;

    // Mirror the asset values so we can watch them on this GameObject's inspector.
    [SerializeField] private int counterMirror;
    [SerializeField] private string messageMirror;

    private void OnEnable()
    {
        data.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        data.OnChanged -= Refresh;
    }

    private void Refresh()
    {
        counterMirror = data.Counter;
        messageMirror = data.Message;
    }
}
