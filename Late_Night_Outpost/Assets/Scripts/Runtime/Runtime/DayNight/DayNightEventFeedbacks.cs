using UnityEngine;
using UnityEngine.Serialization;

namespace Ludocore
{
    /// <summary>Plays fire particles and enables a light between dusk and dawn.</summary>
    public class DayNightEventFeedbacks : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Events")]
        [Tooltip("Fired at dawn — extinguishes the fire.")]
        [FormerlySerializedAs("OnDawnEvent")]
        [SerializeField] private GameEvent onDawn;

        [Tooltip("Fired at dusk — lights the fire.")]
        [FormerlySerializedAs("OnDuskEvent")]
        [SerializeField] private GameEvent onDusk;

        [Header("Feedback")]
        [Tooltip("Particle system played while the fire is lit.")]
        [SerializeField] private ParticleSystem fireParticles;

        [Tooltip("Light enabled while the fire is lit.")]
        [SerializeField] private Light fireLight;

        //==================== LIFECYCLE =====================
        private void OnEnable()
        {
            onDusk.OnRaised += LightFire;
            onDawn.OnRaised += ExtinguishFire;
        }

        private void OnDisable()
        {
            onDusk.OnRaised -= LightFire;
            onDawn.OnRaised -= ExtinguishFire;
        }

        //==================== PRIVATE =====================
        private void LightFire()
        {
            fireParticles.Play();
            fireLight.enabled = true;
        }

        private void ExtinguishFire()
        {
            fireParticles.Stop();
            fireLight.enabled = false;
        }
    }
}
