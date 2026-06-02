using UnityEngine;

namespace Ludocore
{
    /// <summary>Rotates a worldspace canvas (or any transform) to face the camera each frame.</summary>
    public class Billboard : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("What to face. Leave empty to use Camera.main.")]
        [SerializeField] private Transform target;

        [Tooltip("Lock to Y axis only (text stays upright when the camera tilts)")]
        [SerializeField] private bool horizontalOnly = true;

        //==================== LIFECYCLE =====================
        private void LateUpdate()
        {
            if (!target && Camera.main) target = Camera.main.transform;
            if (!target) return;

            Vector3 forward = -target.forward;
            if (horizontalOnly) forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return;

            transform.rotation = Quaternion.LookRotation(forward);
        }

        //==================== INPUTS =====================
        /// <summary>Override the camera/target at runtime.</summary>
        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
