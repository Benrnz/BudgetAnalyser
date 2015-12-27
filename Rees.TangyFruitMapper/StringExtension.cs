using System;

namespace Rees.TangyFruitMapper
{
    internal static class StringExtension
    {
        // Convert the string to camel case.
        public static string ToCamelCase(this string pascalCaseString)
        {
            // TODO this doesnt work
            // If there are 0 or 1 characters, just return the string.
            if (pascalCaseString == null || pascalCaseString.Length < 2)
                return pascalCaseString;

            // Split the string into words.
            var words = pascalCaseString.Split(new char[] {}, StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            var result = words[0].ToLower();
            for (var i = 1; i < words.Length; i++)
            {
                result += words[i].Substring(0, 1).ToUpper() + words[i].Substring(1);
            }

            return result;
        }
    }
}