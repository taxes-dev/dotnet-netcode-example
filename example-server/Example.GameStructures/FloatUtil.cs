using System;

namespace Example.GameStructures
{
    /// <summary>
    /// Float utility functions.
    /// </summary>
    public static class FloatUtil
    {
        /// <summary>
        /// Clamps a floating point value to the given minimum/maximum.
        /// </summary>
        /// <param name="value">A <see cref="Single"/> value.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>A <see cref="Single"/> value between <paramref name="min"/> and <paramref name="max"/>, inclusive.</returns>
        public static float Clamp(this float value, float min, float max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}
