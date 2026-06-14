namespace Ludocore
{
    /// <summary>Which side of a threshold a value must be on.</summary>
    public enum Comparison { Above, Below }

    /// <summary>Small shared helpers used across Ludocore modules.</summary>
    public static class CoreUtils
    {
        /// <summary>
        /// True when <paramref name="value"/> is on the chosen side of <paramref name="threshold"/>.
        /// Above → value &gt; threshold; Below → value &lt; threshold.
        /// </summary>
        public static bool Compare(float value, Comparison comparison, float threshold)
            => comparison == Comparison.Above
                ? value > threshold
                : value < threshold;
    }
}
