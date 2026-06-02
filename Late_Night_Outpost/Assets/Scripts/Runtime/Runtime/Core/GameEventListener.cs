// ============================================
// Game Event Listener
// ============================================
// PURPOSE: Bridges a GameEvent to a UnityEvent response wired in the Inspector.
// USAGE:   Add to a GameObject, assign the event, wire methods to the response.
// ============================================

using UnityEngine;
using UnityEngine.Events;

namespace Ludocore
{
    public class GameEventListener : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Config")]
        [Tooltip("The event this listener subscribes to.")]
        [SerializeField] private GameEvent gameEvent;

        //==================== OUTPUTS =====================
        [Header("Events")]
        [Tooltip("Invoked when the event is raised.")]
        [SerializeField] private UnityEvent response;

        //==================== LIFECYCLE =====================
        // Route through a method on this MonoBehaviour (not response.Invoke directly)
        // so the GameEvent's listener panel can show this component as the subscriber.
        private void OnEnable()  => gameEvent.OnRaised += HandleRaised;
        private void OnDisable() => gameEvent.OnRaised -= HandleRaised;
        private void HandleRaised() => response.Invoke();
    }
}
