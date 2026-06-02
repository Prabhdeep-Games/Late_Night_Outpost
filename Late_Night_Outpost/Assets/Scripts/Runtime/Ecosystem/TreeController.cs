using System;
using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    /// <summary>Tree behavior: matures from seedling to full size as lifecycle energy depletes.</summary>
    public class TreeController : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Modules")]
        [Tooltip("Lifecycle whose energy depletes as the tree matures (full = seedling, empty = mature).")]
        [SerializeField] private Lifecycle lifecycle;

        [Header("Scale")]
        [Tooltip("Scale of a freshly planted seedling.")]
        [Min(0f)]
        [SerializeField] private float seedlingScale = 0.1f;
        [Tooltip("Scale of a fully mature tree.")]
        [Min(0f)]
        [SerializeField] private float matureScale = 1f;

        //==================== STATE =====================
        [Header("Debug")]
        [ReadOnly, SerializeField] private bool isMature;

        public bool IsMature => isMature;

        //==================== OUTPUTS =====================
        public event Action OnMatured;

        [Header("Events")]
        [Tooltip("Fired once when the tree reaches full maturity.")]
        [SerializeField] private UnityEvent maturedEvent;

        //==================== LIFECYCLE =====================
        private void Update()
        {
            float ratio = lifecycle.EnergyRatio;
            ApplyScale(ratio);
            CheckMaturity(ratio);
        }

        //==================== PRIVATE =====================
        private void ApplyScale(float ratio)
        {
            float s = Mathf.Lerp(matureScale, seedlingScale, ratio);
            transform.localScale = new Vector3(s, s, s);
        }

        private void CheckMaturity(float ratio)
        {
            if (isMature) return;
            if (ratio > 0f) return;

            isMature = true;
            OnMatured?.Invoke();
            maturedEvent?.Invoke();
        }
    }
}
