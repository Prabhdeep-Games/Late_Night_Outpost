using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore.Test
{
    /// <summary>Manages building state at a fixed site — tracks level, spawns prefabs, fires events. (TEST)</summary>
    public class BuildingSiteTest : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Building definition with levels and prefabs.")]
        [SerializeField] private BuildingDataTest buildingData;

        [Tooltip("Sensor that detects the player nearby.")]
        [SerializeField] private Sensor sensor;

        [Tooltip("Key to interact with this site.")]
        [SerializeField] private KeyCode interactKey = KeyCode.F;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private int currentLevel = -1;

        private GameObject _currentInstance;

        public int CurrentLevel => currentLevel;
        public bool IsBuilt => currentLevel >= 0;
        public bool IsMaxLevel => buildingData && currentLevel >= buildingData.LevelCount - 1;
        public BuildingDataTest Data => buildingData;

        //==================== OUTPUTS =====================
        public event Action<int> OnBuilt;
        public event Action OnBuildFailed;

        [Header("Events")]
        [Tooltip("Fired when a level is built or upgraded. Passes new level index.")]
        [SerializeField] private UnityEvent<int> builtEvent;

        [Tooltip("Fired when building fails (max level reached or missing resources).")]
        [SerializeField] private UnityEvent buildFailedEvent;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            if (!sensor || !sensor.HasDetections) return;
            if (!Input.GetKeyDown(interactKey)) return;

            Build();
        }

        //==================== INPUTS =====================
        /// <summary>Attempt to build or upgrade. Fires OnBuilt on success, OnBuildFailed otherwise.</summary>
        public void Build()
        {
            if (IsMaxLevel)
            {
                OnBuildFailed?.Invoke();
                buildFailedEvent?.Invoke();
                return;
            }

            // Resource check will go here

            currentLevel++;

            // Destroy previous level instance
            if (_currentInstance) Destroy(_currentInstance);

            // Spawn new level prefab
            var level = buildingData.GetLevel(currentLevel);
            if (level.Prefab)
            {
                _currentInstance = Instantiate(level.Prefab, transform.position, transform.rotation, transform);
            }

            OnBuilt?.Invoke(currentLevel);
            builtEvent?.Invoke(currentLevel);
        }
    }
}
