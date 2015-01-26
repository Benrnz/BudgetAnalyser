using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// An extension class for string.
    /// </summary>
    public static class StringExtension
    {
        private static readonly char[] Vowels = { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' };

        /// <summary>
        /// Returns the appropriate 'An' or 'A' prefix for the given string.  The given string is not included
        /// in the return text.
        /// </summary>
        /// <param name="instance">The string to determine the prefix for.</param>
        /// <param name="properCase">A boolean to indicate if the prefix should be proper-cased or not.</param>
        public static string AnOrA(this string instance, bool properCase = false)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                return instance;
            }

            bool useAn = Vowels.Contains(instance.ToCharArray(0, 1)[0]);

            if (properCase && useAn)
            {
                return "An";
            }

            if (properCase)
            {
                return "A";
            }

            if (useAn)
            {
                return "an";
            }

            return "a";
        }

        /// <summary>
        /// Truncates the right of a string to the specified length, but only if it exceeds that length. Optionally the returned string can include
        /// ellipses.
        /// </summary>
        public static string Truncate(this string instance, int truncateToLength, bool useEllipses = false)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                return string.Empty;
            }

            if (instance.Length <= truncateToLength)
            {
                return instance;
            }

            if (useEllipses)
            {
                return instance.Substring(0, truncateToLength - 1) + "…";
            }

            return instance.Substring(0, truncateToLength);
        }

        /// <summary>
        /// Truncates the left of a string to the specified length, but only if it exceeds that length. Optionally the returned string can
        /// include ellipses.
        /// </summary>
        public static string TruncateLeft(this string instance, int truncateToLength, bool useEllipses = false)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                return string.Empty;
            }

            if (instance.Length <= truncateToLength)
            {
                return instance;
            }

            if (useEllipses)
            {
                return "…" + instance.Substring(instance.Length - truncateToLength + 1, truncateToLength - 1);
            }

            return instance.Substring(instance.Length - truncateToLength, truncateToLength);
        }

        /// <summary>
        /// Trims the end of a string safely.  If the string is null, null is returned.
        /// </summary>
        public static string TrimEndSafely(this string instance)
        {
            if (instance == null) return null;
            return instance.TrimEnd();
        }
    }
}