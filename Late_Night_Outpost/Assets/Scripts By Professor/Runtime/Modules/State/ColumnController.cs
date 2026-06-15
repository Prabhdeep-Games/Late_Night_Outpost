using UnityEngine;

/// <summary>Maps proximity sensor distance to emission intensity, audio pitch, and light intensity — closer means brighter, higher pitch, brighter light.</summary>
public class ColumnController : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("Proximity sensor that detects the target")]
    [SerializeField] private Ludocore.ProximitySensor sensor;

    [Header("Renderer")]
    [Tooltip("Renderer whose material emission to drive")]
    [SerializeField] private Renderer targetRenderer;
    [Tooltip("Material index on the renderer")]
    [Min(0)]
    [SerializeField] private int materialIndex;

    [Header("Emission")]
    [Tooltip("Base emission color")]
    [SerializeField, ColorUsage(false, true)] private Color emissionColor = Color.white;
    [Tooltip("Emission intensity when the object is at closest distance")]
    [Min(0f)]
    [SerializeField] private float maxEmissionIntensity = 2f;
    [Tooltip("Emission intensity when nothing is detected or at max distance")]
    [Min(0f)]
    [SerializeField] private float minEmissionIntensity = 0f;

    [Header("Audio")]
    [Tooltip("Audio source whose pitch to drive")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Pitch when the object is at closest distance")]
    [Min(0.01f)]
    [SerializeField] private float maxPitch = 2f;
    [Tooltip("Pitch when nothing is detected or at max distance")]
    [Min(0.01f)]
    [SerializeField] private float minPitch = 0.5f;

    [Header("Light")]
    [Tooltip("Point light whose intensity to drive")]
    [SerializeField] private Light pointLight;
    [Tooltip("Light intensity when the object is at closest distance")]
    [Min(0f)]
    [SerializeField] private float maxLightIntensity = 2f;
    [Tooltip("Light intensity when nothing is detected or at max distance")]
    [Min(0f)]
    [SerializeField] private float minLightIntensity = 0f;

    [Header("Mapping")]
    [Tooltip("Maximum detection distance (proximity 0 at this distance)")]
    [Min(0.01f)]
    [SerializeField] private float maxDistance = 5f;
    [Tooltip("Remapping curve (left=farthest, right=closest)")]
    [SerializeField] private AnimationCurve responseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private Material _material;

    private void Start()
    {
        if (!targetRenderer) return;

        var materials = targetRenderer.materials;
        if (materialIndex < 0 || materialIndex >= materials.Length) return;

        _material = materials[materialIndex];
        _material.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        if (!sensor) return;

        float proximity = 0f;

        if (sensor.TryGetNearest(out var nearest))
        {
            proximity = 1f - Mathf.Clamp01(nearest.Distance / maxDistance);
            proximity = responseCurve.Evaluate(proximity);
        }

        // Emission
        if (_material)
        {
            float emissionIntensity = Mathf.Lerp(minEmissionIntensity, maxEmissionIntensity, proximity);
            _material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
        }

        // Audio Pitch
        if (audioSource)
        {
            audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, proximity);
        }

        // Light Intensity
        if (pointLight)
        {
            pointLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, proximity);
        }
    }

    private void OnDestroy()
    {
        if (_material && Application.isPlaying)
        {
            Destroy(_material);
        }
    }
}
