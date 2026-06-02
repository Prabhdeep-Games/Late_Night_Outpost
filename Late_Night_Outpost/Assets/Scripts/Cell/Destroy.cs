using System;
using UnityEngine;
using UnityEngine.Events;
    /// <summary>Destroys this GameObject, optionally after a delay.</summary>
    public class Destroy : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("Delay in seconds before destruction (0 = instant)")]
        [Min(0f)]
        [SerializeField] private float delay;

        [SerializeField] private bool autoPlay = false;

        //==================== OUTPUTS =====================
        public event Action OnDestroying;

        [Header("Events")]
        [Tooltip("Fired just before the GameObject is destroyed.")]
        [SerializeField] private UnityEvent destroyingEvent;

        private void OnEnable()
        {
            if(autoPlay) DestroySelf();
        }

        //==================== INPUTS =====================
        /// <summary>Destroy this GameObject.</summary>
        [ContextMenu("Destroy Self")]
        public void DestroySelf()
        {
            OnDestroying?.Invoke();
            destroyingEvent?.Invoke();
            Destroy(gameObject, delay);
        }
    }
    
