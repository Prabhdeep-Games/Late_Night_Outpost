using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Thin wrapper around NavMeshAgent — provides clean movement verbs. Drop it on a GameObject with a target set + autoPlay on for prototyping.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMotor : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Optional destination Transform. Used by autoPlay and MoveToTarget().")]
        [SerializeField] private Transform target;

        [Tooltip("On enable, move to the target's position once. Composers should leave this off.")]
        [SerializeField] private bool autoPlay;

        //==================== STATE =====================
        private NavMeshAgent _agent;
        private bool _wasMoving;
        private bool _explicitStop;

        [Header("Debug")]
        [ReadOnly, SerializeField] private bool isMoving;

        public bool IsMoving => isMoving;

        public bool HasArrived => _agent.isOnNavMesh
            && !_agent.pathPending
            && _agent.remainingDistance <= _agent.stoppingDistance;

        public float Speed => _agent.speed;

        //==================== OUTPUTS =====================
        public event Action OnStartedMoving;
        public event Action OnStoppedMoving;
        public event Action OnArrived;

        [Header("Events")]
        [Tooltip("Fired when the agent starts moving")]
        [SerializeField] private UnityEvent startedMovingEvent;
        [Tooltip("Fired when movement ends due to an explicit Stop() call")]
        [SerializeField] private UnityEvent stoppedMovingEvent;
        [Tooltip("Fired when the agent reaches its destination naturally")]
        [SerializeField] private UnityEvent arrivedEvent;

        //==================== LIFECYCLE =====================
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()
        {
            if (autoPlay) MoveToTarget();
        }

        private void Update()
        {
            bool moving = _agent.isOnNavMesh
                && !_agent.pathPending
                && _agent.remainingDistance > _agent.stoppingDistance;

            if (moving && !_wasMoving)
            {
                OnStartedMoving?.Invoke();
                startedMovingEvent?.Invoke();
            }
            else if (!moving && _wasMoving)
            {
                if (_explicitStop)
                {
                    OnStoppedMoving?.Invoke();
                    stoppedMovingEvent?.Invoke();
                }
                else
                {
                    OnArrived?.Invoke();
                    arrivedEvent?.Invoke();
                }
                _explicitStop = false;
            }

            _wasMoving = moving;
            isMoving = moving;
        }

        //==================== INPUTS =====================
        /// <summary>Set a NavMesh destination.</summary>
        public void MoveTo(Vector3 position)
        {
            if (!_agent.isOnNavMesh) return;

            _explicitStop = false;
            _agent.isStopped = false;
            _agent.SetDestination(position);
        }

        /// <summary>Move to a Transform's current position (one-shot, not tracked).</summary>
        public void MoveTo(Transform t)
        {
            if (!t) return;
            MoveTo(t.position);
        }

        /// <summary>Move to the configured target Transform.</summary>
        [ContextMenu("Move To Target")]
        public void MoveToTarget()
        {
            if (!target) return;
            MoveTo(target.position);
        }

        /// <summary>Stop movement and clear the current path. Triggers OnStoppedMoving on the next transition (not OnArrived).</summary>
        [ContextMenu("Stop")]
        public void Stop()
        {
            if (!_agent.isOnNavMesh) return;

            _explicitStop = true;
            _agent.isStopped = true;
            _agent.ResetPath();
        }

        /// <summary>Replace the target Transform at runtime.</summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        //==================== PRIVATE =====================
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !_agent || !_agent.hasPath) return;

            Gizmos.color = Color.yellow;
            var corners = _agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
                Gizmos.DrawLine(corners[i], corners[i + 1]);

            Gizmos.DrawSphere(_agent.destination, 0.2f);
        }
    }
}
