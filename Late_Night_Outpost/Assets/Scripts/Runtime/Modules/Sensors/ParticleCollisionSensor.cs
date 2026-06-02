using UnityEngine;

namespace Ludocore
{
    /// <summary>Detects objects hit by this ParticleSystem's particles. Requires Collision module ON and Send Collision Messages ON.</summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleCollisionSensor : Sensor
    {
        //==================== LIFECYCLE =====================
        // Particle impacts are transient events, not presence. We add on impact,
        // clear at end of frame, so OnSignalAdded fires for every fresh impact
        // and consumers see "what got hit this frame" in the signal list.
        private void LateUpdate()
        {
            RefreshDistances();
            for (int i = Signals.Count - 1; i >= 0; i--)
                RemoveDetection(Signals[i].Object);
        }

        private void OnParticleCollision(GameObject other)
        {
            if (IsDetected(other)) return;

            AddDetection(new Signal
            {
                Object = other,
                Distance = Vector3.Distance(transform.position, other.transform.position)
            });
        }
    }
}
