// ============================================
// Day Night View
// ============================================
// Reads TimeOfDay and drives sun rotation, sun intensity,
// procedural skybox exposure, and ambient intensity.
// ============================================

using UnityEngine;

namespace Ludocore
{
    public class DayNightView : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Source")]
        [Tooltip("Normalized time of day (0..1). 0 = midnight, 0.5 = noon.")]
        [SerializeField] private FloatVariable timeOfDay;

        [Header("Sun")]
        [Tooltip("Directional light rotated and dimmed by the cycle.")]
        [SerializeField] private Light sun;

        [Tooltip("Compass direction of the sun's path (Y rotation).")]
        [Range(0f, 360f)]
        [SerializeField] private float sunYaw = 30f;

        [Header("Brightness")]
        [Tooltip("Brightness factor across the cycle. X = TimeOfDay (0..1), Y = factor (0..1). " +
                 "Drives sun intensity, skybox exposure, and ambient intensity together.")]
        [SerializeField] private AnimationCurve daylightCurve = new AnimationCurve(
            new Keyframe(0f,    0f),
            new Keyframe(0.25f, 0f),
            new Keyframe(0.5f,  1f),
            new Keyframe(0.75f, 0f),
            new Keyframe(1f,    0f)
        );

        [Header("Sun Intensity")]
        [Min(0f)]
        [Tooltip("Sun intensity at full night (brightness = 0).")]
        [SerializeField] private float minSunIntensity = 0f;

        [Min(0f)]
        [Tooltip("Sun intensity at full daylight (brightness = 1).")]
        [SerializeField] private float maxSunIntensity = 1.2f;

        [Header("Skybox Exposure")]
        [Min(0f)]
        [Tooltip("Procedural skybox exposure at full night.")]
        [SerializeField] private float minSkyboxExposure = 0.2f;

        [Min(0f)]
        [Tooltip("Procedural skybox exposure at full daylight.")]
        [SerializeField] private float maxSkyboxExposure = 1.3f;

        [Header("Ambient Intensity")]
        [Range(0f, 8f)]
        [Tooltip("Environment lighting multiplier at full night.")]
        [SerializeField] private float minAmbientIntensity = 0.2f;

        [Range(0f, 8f)]
        [Tooltip("Environment lighting multiplier at full daylight.")]
        [SerializeField] private float maxAmbientIntensity = 1f;

        //==================== STATE =====================
        // Runtime instance of the skybox material so we don't dirty the shared asset on disk.
        private Material _skyboxInstance;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            if (RenderSettings.skybox is not null)
            {
                _skyboxInstance = new Material(RenderSettings.skybox);
                RenderSettings.skybox = _skyboxInstance;
            }
        }

        private void LateUpdate() => Apply();

        private void OnDestroy()
        {
            if (_skyboxInstance is not null) Destroy(_skyboxInstance);
        }

        //==================== PRIVATE =====================
        private void Apply()
        {
            float t = timeOfDay.Value;
            float brightness = Mathf.Clamp01(daylightCurve.Evaluate(t));

            sun.transform.rotation = Quaternion.Euler(t * 360f - 90f, sunYaw, 0f);
            sun.intensity = Mathf.Lerp(minSunIntensity, maxSunIntensity, brightness);

            if (_skyboxInstance is not null)
                _skyboxInstance.SetFloat("_Exposure", Mathf.Lerp(minSkyboxExposure, maxSkyboxExposure, brightness));

            RenderSettings.ambientIntensity = Mathf.Lerp(minAmbientIntensity, maxAmbientIntensity, brightness);
        }
    }
}
