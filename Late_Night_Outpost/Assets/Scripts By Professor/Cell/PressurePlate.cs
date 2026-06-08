using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Pressure plate: presses down while a Sensor detects something on it; releases when empty.</summary>
    public class PressurePlate : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Sensor producing the detection signals. Swap the type (TriggerSensor, ProximitySensor, …) to change how the plate is triggered.")]
        [SerializeField] private Sensor source;

        [Tooltip("Transform that moves when the plate is pressed. Defaults to this transform.")]
        [SerializeField] private Transform plate;

        [Tooltip("Local offset applied while pressed (typically a negative Y).")]
        [SerializeField] private Vector3 pressedOffset = new(0f, -0.1f, 0f);

        [Tooltip("Animation duration in seconds.")]
        [Min(0f)]
        [SerializeField] private float duration = 0.2f;

        [Tooltip("Tween easing.")]
        [SerializeField] private Ease ease = Ease.OutQuad;

        //==================== VISUALS =====================
        [Header("Visuals")]
        [Tooltip("Renderer whose emission flips on with activation. Leave empty to skip the visual.")]
        [SerializeField] private Renderer targetRenderer;

        [Tooltip("Emissive color applied while the plate is pressed.")]
        [ColorUsage(true, true)]
        [SerializeField] private Color activeEmission = Color.white;

        //==================== STATE LINK =====================
        [Header("State")]
        [Tooltip("Optional BoolVariable mirrored to the plate's pressed state.")]
        [SerializeField] private BoolVariable isActive;

        //==================== OUTPUTS =====================
        public event Action OnActivated;
        public event Action OnDeactivated;

        [Header("Events")]
        [Tooltip("Invoked locally when the plate first becomes pressed.")]
        [SerializeField] private UnityEvent activatedEvent;

        [Tooltip("Invoked locally when the plate becomes fully empty.")]
        [SerializeField] private UnityEvent deactivatedEvent;

        [Tooltip("Optional broadcast event raised when the plate first becomes pressed.")]
        [SerializeField] private GameEvent activatedGameEvent;

        [Tooltip("Optional broadcast event raised when the plate becomes fully empty.")]
        [SerializeField] private GameEvent deactivatedGameEvent;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private bool pressed;

        private Vector3 _restPosition;
        private Material _material;
        private Tween _tween;

        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            if (!plate) plate = transform;
            _restPosition = plate.localPosition;

            if (targetRenderer)
            {
                _material = targetRenderer.material;
                _material.EnableKeyword("_EMISSION");
                _material.SetColor(EmissionColorId, Color.black);
            }
        }

        private void OnEnable()
        {
            source.OnSignalAdded += HandleSignalAdded;
            source.OnSignalLost  += HandleSignalLost;
        }

        private void OnDisable()
        {
            source.OnSignalAdded -= HandleSignalAdded;
            source.OnSignalLost  -= HandleSignalLost;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
            if (_material && Application.isPlaying) Destroy(_material);
        }

        //==================== PRIVATE =====================
        private void HandleSignalAdded(Signal s)
        {
            if (pressed) return;
            SetPressed(true);
        }

        private void HandleSignalLost(Signal s)
        {
            if (!pressed || source.HasDetections) return;
            SetPressed(false);
        }

        private void SetPressed(bool next)
        {
            pressed = next;

            _tween?.Kill();
            Vector3 target = next ? _restPosition + pressedOffset : _restPosition;
            _tween = plate.DOLocalMove(target, duration).SetEase(ease);

            if (_material)
                _material.SetColor(EmissionColorId, next ? activeEmission : Color.black);

            if (isActive) isActive.Value = next;

            if (next)
            {
                OnActivated?.Invoke();
                activatedEvent?.Invoke();
                if (activatedGameEvent) activatedGameEvent.Raise();
            }
            else
            {
                OnDeactivated?.Invoke();
                deactivatedEvent?.Invoke();
                if (deactivatedGameEvent) deactivatedGameEvent.Raise();
            }
        }
    }
}
