// ============================================================================
// Curve Data
// ============================================================================
// A ScriptableObject wrapping a single AnimationCurve so a curve becomes a
// shareable, named asset. Drag it into any system that wants to read a value
// driven by another (e.g. "enemies per portal driven by day count").
//
// USAGE
//   Create asset:  Right-click → Create → Ludocore → Data → Curve
//   Name it semantically:  PortalCountPerNight.asset, EnemiesPerPortal.asset
//   Reference it:  [SerializeField] private CurveData portalCountCurve;
//   Read it:       int n = Mathf.RoundToInt(portalCountCurve.Evaluate(day));
//
// The curve editor in the inspector is where designers tune the ramp. No code
// changes needed to retune difficulty.
// ============================================================================

using UnityEngine;

namespace Ludocore
{
    [CreateAssetMenu(fileName = "NewCurveData", menuName = "Ludocore/Data/Curve")]
    public class CurveData : ScriptableObject
    {
        [Tooltip("The curve. Designer drags keyframes in the inspector to shape the ramp.")]
        public AnimationCurve Curve = AnimationCurve.Linear(0f, 1f, 10f, 5f);

        /// <summary>Evaluate the curve at t. Returns the y-value at that x-position.</summary>
        public float Evaluate(float t) => Curve.Evaluate(t);
    }
}
