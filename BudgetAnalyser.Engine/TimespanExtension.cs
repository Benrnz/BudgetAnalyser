using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A few extensions to assist in creating <see cref="TimeSpan" />s, and working with <see cref="TimeSpan" />s.
    /// </summary>
    public static class TimeSpanExtension
    {
        /// <summary>
        ///     Creates a <see cref="TimeSpan" /> from the given number value in seconds.
        /// </summary>
        public static TimeSpan Seconds(this int instance)
        {
            return TimeSpan.FromSeconds(instance);
        }

        /// <summary>
        ///     Creates a <see cref="TimeSpan" /> from the given number value in seconds.
        /// </summary>
        public static TimeSpan Seconds(this long instance)
        {
            return TimeSpan.FromSeconds(instance);
        }
    }
}