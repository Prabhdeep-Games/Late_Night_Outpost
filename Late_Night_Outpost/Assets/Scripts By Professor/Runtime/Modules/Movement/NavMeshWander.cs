using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Picks a random NavMesh point when the motor is idle.</summary>
    [RequireComponent(typeof(NavMeshMotor))]
    public class NavMeshWander : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Max wander radius around the current position")]
        [Min(0f)]
        [SerializeField] private float radius = 10f;

        [Tooltip("How long to wait at each destination before picking a new one")]
        [Min(0f)]
        [SerializeField] private float pauseTime = 0.5f;

        [Tooltip("Start wandering on enable")]
        [SerializeField] private bool autoPlay = true;

        //==================== STATE =====================
        private NavMeshMotor _motor;
        private float _pauseTimer;

        [Header("Debug")]
        [ReadOnly, SerializeField] private bool isWandering;

        public bool IsWandering => isWandering;
        public bool IsWaiting => _pauseTimer > 0f;

        //==================== OUTPUTS =====================
        public event Action OnWanderStarted;
        public event Action OnWanderStopped;
        public event Action<Vector3> OnNewDestination;

        [Header("Events")]
        [Tooltip("Fired when wandering begins")]
        [SerializeField] private UnityEvent wanderStartedEvent;
        [Tooltip("Fired when wandering stops")]
        [SerializeField] private UnityEvent wanderStoppedEvent;
        [Tooltip("Fired when a new wander destination is chosen")]
        [SerializeField] private UnityEvent<Vector3> newDestinationEvent;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            _motor = GetComponent<NavMeshMotor>();
        }

        private void OnEnable()
        {
            if (autoPlay) StartWander();
        }

        private void OnDisable()
        {
            if (isWandering) StopWander();
        }

        private void Update()
        {
            if (!isWandering) return;
            if (!_motor.HasArrived) return;

            _pauseTimer -= Time.deltaTime;
            if (_pauseTimer > 0f) return;

            PickNewDestination();
            _pauseTimer = pauseTime;
        }

        //==================== INPUTS =====================
        /// <summary>Begin wandering — picks a new destination immediately.</summary>
        [ContextMenu("Start Wander")]
        public void StartWander()
        {
            isWandering = true;
            _pauseTimer = 0f;
            OnWanderStarted?.Invoke();
            wanderStartedEvent?.Invoke();
        }

        /// <summary>Stop wandering and halt movement.</summary>
        [ContextMenu("Stop Wander")]
        public void StopWander()
        {
            isWandering = false;
            _motor.Stop();
            OnWanderStopped?.Invoke();
            wanderStoppedEvent?.Invoke();
        }

        //==================== PRIVATE =====================
        private void PickNewDestination()
        {
            var randomDirection = UnityEngine.Random.insideUnitSphere * radius + transform.position;

            if (!NavMesh.SamplePosition(randomDirection, out var hit, radius, NavMesh.AllAreas)) return;

            _motor.MoveTo(hit.position);
            OnNewDestination?.Invoke(hit.position);
            newDestinationEvent?.Invoke(hit.position);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
