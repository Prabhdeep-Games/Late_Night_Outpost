using UnityEngine;

public class FloatAndRotate : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Degrees per second around the Y axis.")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Floating")]
    [Tooltip("Height of the up/down movement.")]
    [SerializeField] private float floatAmplitude = 0.25f;

    [Tooltip("Speed of the up/down movement.")]
    [SerializeField] private float floatFrequency = 1f;

    private Vector3 _startPosition;

    private void Awake()
    {
        _startPosition = transform.position;
    }

    private void Update()
    {
        // Rotate around Y.
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);

        // Float up and down using a sine wave.
        float offset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = _startPosition + new Vector3(0f, offset, 0f);
    }
}