using System;
using UnityEngine;

// The asset holds shared state AND notifies subscribers when it changes.
// Sender writes through the properties — Receiver subscribes to OnChanged.
// Neither references the other; both reference the same asset.
// Note: edits to SO assets persist across Play sessions in the editor.
[CreateAssetMenu(fileName = "SharedData", menuName = "Exercises/Shared Data")]
public class SharedData : ScriptableObject
{
    [SerializeField] private int counter;
    [SerializeField] private string message;

    public event Action OnChanged;

    public int Counter
    {
        get => counter;
        set
        {
            if (counter == value) return;
            counter = value;
            OnChanged?.Invoke();
        }
    }

    public string Message
    {
        get => message;
        set
        {
            if (message == value) return;
            message = value;
            OnChanged?.Invoke();
        }
    }
}
