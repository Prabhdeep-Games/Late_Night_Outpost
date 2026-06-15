namespace Ludocore
{
    /// <summary>Which side of a threshold a value must be on. The "AtOr" variants include the threshold itself — use AtOrBelow with threshold 0 to catch "exactly 0".</summary>
    public enum Comparison { Above, Below, AtOrAbove, AtOrBelow }

    /// <summary>Small shared helpers used across Ludocore modules.</summary>
    public static class CoreUtils
    {
        /// <summary>True when <paramref name="value"/> is on the chosen side of <paramref name="threshold"/>.</summary>
        public static bool Compare(float value, Comparison comparison, float threshold)
        {
            switch (comparison)
            {
                case Comparison.Above:     return value >  threshold;
                case Comparison.Below:     return value <  threshold;
                case Comparison.AtOrAbove: return value >= threshold;
                case Comparison.AtOrBelow: return value <= threshold;
                default:                   return false;
            }
        }
    }
}
