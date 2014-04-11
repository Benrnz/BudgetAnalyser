using System.Linq;

namespace BudgetAnalyser.Engine
{
    public static class StringExtension
    {
        private static char[] Vowels = new[] { 'a', 'e', 'i', 'o', 'u', 'A','E', 'I', 'O', 'U' };

        public static string AOrAn(this string instance, bool properCase = false)
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

        public static string Truncate(this string instance, int truncateToLength, ref bool isEmpty, bool useEllipses = false, string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(instance))
            {
                isEmpty = true;
                return string.Empty;
            }

            string returnValue;
            if (instance.Length <= truncateToLength)
            {
                returnValue = isEmpty ? instance : prefix + instance;
            }
            else
            {
                if (useEllipses)
                {
                    returnValue = isEmpty ? instance.Substring(0, truncateToLength - 1) + "…" : prefix + instance.Substring(0, truncateToLength - 1) + "…";
                }
                else
                {
                    returnValue = isEmpty ? instance.Substring(0, truncateToLength) : prefix + instance.Substring(0, truncateToLength);
                }
            }

            isEmpty = false;
            return returnValue;
        }

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
    }
}