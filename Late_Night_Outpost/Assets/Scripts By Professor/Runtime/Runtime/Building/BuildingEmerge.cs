// ============================================================================
// BuildingEmerge — Presentation
//
// Sinks the building below its authored Y on Start, then tweens it back up.
// Lives on the spawned building prefab so every building gets the same
// rise-from-ground intro automatically — regardless of whether it was
// placed by a BuildingSite on terrain or by a held Builder.
//
// By default animates the GameObject this component is on. For multi-piece
// buildings where each piece should emerge independently, drag the piece
// Transforms into the pieces list.
// ============================================================================

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Ludocore
{
    /// <summary>Sinks and rises pieces (or the building root) on Start for a
    /// "emerge from the ground" intro.</summary>
    public class BuildingEmerge : MonoBehaviour
    {
        //==================== CONFIG =====================
        [Header("Pieces")]
        [Tooltip("Transforms to animate. Leave empty to animate this GameObject's transform.")]
        [SerializeField] private List<Transform> pieces = new();

        [Header("Animation")]
        [Tooltip("How far below the final Y each piece starts before tweening up.")]
        [Min(0f)]
        [SerializeField] private float emergeDistance = 2f;

        [Tooltip("Duration of the tween for each piece.")]
        [Min(0f)]
        [SerializeField] private float emergeDuration = 0.6f;

        [Tooltip("Easing curve.")]
        [SerializeField] private Ease emergeEase = Ease.OutBack;

        //==================== LIFECYCLE =====================
        private void Start()
        {
            if (pieces.Count == 0)
            {
                Emerge(transform);
                return;
            }

            foreach (Transform t in pieces)
                if (t) Emerge(t);
        }

        //==================== PRIVATE =====================
        private void Emerge(Transform t)
        {
            float finalY = t.localPosition.y;
            Vector3 start = t.localPosition;
            start.y = finalY - emergeDistance;
            t.localPosition = start;
            t.DOLocalMoveY(finalY, emergeDuration).SetEase(emergeEase);
        }
    }
}

// ============================================================================
// Setup on a building prefab
//   1. Add this BuildingEmerge component anywhere on the prefab (typically the root).
//   2. Either leave pieces empty (the root tweens as a whole — simplest)
//      or drag individual piece Transforms for a more layered emerge.
//   3. Tune emergeDistance / emergeDuration / emergeEase to taste.
// ============================================================================
