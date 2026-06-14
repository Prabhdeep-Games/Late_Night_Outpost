using System.Collections.Generic;
using UnityEngine;

namespace Ludocore
{
    /// <summary>Detects objects in range, within field of view, and with unobstructed line of sight.</summary>
    public class SightSensor : Sensor
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Maximum detection range"), Min(0f)]
        [SerializeField] private float range = 15f;

        [Tooltip("Field of view in degrees. 360 = all-around (no FOV check)")]
        [Range(0f, 360f)]
        [SerializeField] private float fov = 120f;

        [Tooltip("Vertical offset for the eye position (raycast origin)"), Min(0f)]
        [SerializeField] private float eyeHeight = 1.5f;

        [Tooltip("Which layers can be detected (broad-phase)")]
        [SerializeField] private LayerMask detectionLayers = ~0;

        [Tooltip("Layers that block line of sight. Empty = no LOS check")]
        [SerializeField] private LayerMask obstacleLayers;

        [Tooltip("Seconds between detection checks. 0 = every frame"), Min(0f)]
        [SerializeField] private float checkInterval = 0.1f;

        //==================== STATE =====================
        private readonly Collider[] _overlapBuffer = new Collider[64];
        private readonly HashSet<GameObject> _currentFrame = new();
        private float _nextCheckTime;

        public Vector3 EyePosition => transform.position + Vector3.up * eyeHeight;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + checkInterval;

            Vector3 eye = EyePosition;
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, range, _overlapBuffer, detectionLayers);

            _currentFrame.Clear();
            for (int i = 0; i < count; i++)
            {
                GameObject obj = _overlapBuffer[i].gameObject;
                if (obj == gameObject) continue;
                if (!IsInFov(obj)) continue;
                if (!HasLineOfSight(eye, obj)) continue;
                _currentFrame.Add(obj);
            }

            RemoveLost();
            AddNew();
            RefreshDistances();
        }

        //==================== PRIVATE =====================
        private bool IsInFov(GameObject obj)
        {
            if (fov >= 360f) return true;

            Vector3 toTarget = obj.transform.position - transform.position;
            Vector3 flatDir = new Vector3(toTarget.x, 0f, toTarget.z);
            if (flatDir.sqrMagnitude < 0.0001f) return true;
            flatDir.Normalize();

            Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
            float angle = Vector3.Angle(flatForward, flatDir);
            return angle <= fov * 0.5f;
        }

        private bool HasLineOfSight(Vector3 eye, GameObject obj)
        {
            if (obstacleLayers.value == 0) return true;

            Vector3 targetPos = obj.transform.position + Vector3.up * eyeHeight * 0.5f;
            Vector3 dir = targetPos - eye;
            float dist = dir.magnitude;
            if (dist < 0.0001f) return true;

            // Any obstacle hit before reaching the target = blocked
            return !Physics.Raycast(eye, dir / dist, dist, obstacleLayers);
        }

        private void RemoveLost()
        {
            for (int i = Signals.Count - 1; i >= 0; i--)
            {
                if (!_currentFrame.Contains(Signals[i].Object))
                    RemoveDetection(Signals[i].Object);
            }
        }

        private void AddNew()
        {
            foreach (var obj in _currentFrame)
            {
                if (IsDetected(obj)) continue;

                AddDetection(new Signal
                {
                    Object = obj,
                    Distance = Vector3.Distance(transform.position, obj.transform.position)
                });
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position;
            Vector3 eye = origin + Vector3.up * eyeHeight;

            // Range sphere
            Gizmos.color = HasDetections ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(origin, range);

            // FOV cone
            DrawFovCone(eye);

            // Eye marker
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(eye, 0.1f);

            // Lines to currently detected signals
            Gizmos.color = Color.green;
            for (int i = 0; i < Signals.Count; i++)
            {
                if (!Signals[i].Object) continue;
                Vector3 targetEye = Signals[i].Object.transform.position + Vector3.up * eyeHeight * 0.5f;
                Gizmos.DrawLine(eye, targetEye);
                Gizmos.DrawWireSphere(targetEye, 0.2f);
            }
        }

        private void DrawFovCone(Vector3 eye)
        {
            if (fov >= 360f) return;

            Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.7f);

            float halfFov = fov * 0.5f;
            Vector3 leftDir = Quaternion.Euler(0f, -halfFov, 0f) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0f, halfFov, 0f) * transform.forward;

            Gizmos.DrawRay(eye, leftDir * range);
            Gizmos.DrawRay(eye, rightDir * range);

            const int segments = 24;
            Vector3 prev = eye + leftDir * range;
            for (int i = 1; i <= segments; i++)
            {
                float angle = -halfFov + (fov * i / segments);
                Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
                Vector3 point = eye + dir * range;
                Gizmos.DrawLine(prev, point);
                prev = point;
            }
        }
    }
}
