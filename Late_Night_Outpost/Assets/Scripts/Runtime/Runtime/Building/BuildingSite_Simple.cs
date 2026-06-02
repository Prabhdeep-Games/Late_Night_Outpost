using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>SIMPLE (L4) version. Self-contained: proximity sensor + keycode, no interfaces.
    /// Kept so the Lecture 4 scene keeps working. New scripts use BuildingSite + Focusable + IBuildable.</summary>
    public class BuildingSite_Simple : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Building definition with the prefab to spawn.")]
        [SerializeField] private BuildingData_Simple buildingData;

        [Tooltip("Sensor that detects the player nearby (use a TriggerSensor).")]
        [SerializeField] private Sensor sensor;

        [Tooltip("Key to press to trigger building while the player is detected.")]
        [SerializeField] private KeyCode buildKey = KeyCode.F;

        [Header("Appear Animation")]
        [Tooltip("Duration of the scale-up animation when the building spawns.")]
        [SerializeField] private float appearDuration = 0.6f;

        [Tooltip("Easing curve for the appear animation.")]
        [SerializeField] private Ease appearEase = Ease.OutBack;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private bool playerDetected;
        [ReadOnly, SerializeField] private bool isBuilt;

        private bool _hadSignal;

        public BuildingData_Simple Data => buildingData;
        public bool PlayerDetected => playerDetected;
        public bool IsBuilt => isBuilt;

        //==================== OUTPUTS =====================
        public event Action OnPlayerDetected;
        public event Action OnPlayerLost;

        [Header("Events")]
        [Tooltip("Fired when the player enters detection range.")]
        [SerializeField] private UnityEvent playerDetectedEvent;

        [Tooltip("Fired when the player leaves detection range.")]
        [SerializeField] private UnityEvent playerLostEvent;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            if (!sensor) return;

            bool hasSignal = sensor.HasDetections;

            if (hasSignal && !_hadSignal)
            {
                playerDetected = true;
                OnPlayerDetected?.Invoke();
                playerDetectedEvent?.Invoke();
            }
            else if (!hasSignal && _hadSignal)
            {
                playerDetected = false;
                OnPlayerLost?.Invoke();
                playerLostEvent?.Invoke();
            }

            _hadSignal = hasSignal;

            if (playerDetected && Input.GetKeyDown(buildKey))
                Build();
        }

        //==================== INPUTS =====================
        /// <summary>Spawn the building prefab at this site's position.</summary>
        [ContextMenu("Build")]
        public void Build()
        {
            if (isBuilt) return;
            if (!buildingData) return;
            if (!buildingData.Prefab) return;

            GameObject building = Instantiate(buildingData.Prefab, transform.position, transform.rotation, transform);
            building.transform.localScale = Vector3.zero;
            building.transform.DOScale(Vector3.one, appearDuration).SetEase(appearEase);
            isBuilt = true;
        }
    }
}
