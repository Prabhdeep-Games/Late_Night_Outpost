using UnityEngine;

public class SingletonReceiver : MonoBehaviour
{
    // The single instance everyone talks to.
    public static SingletonReceiver Instance;

    public int counter;
    public string message;

    // Awake only runs in Play mode, so Instance is null in Edit mode.
    private void Awake() => Instance = this;
}
