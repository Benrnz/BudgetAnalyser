namespace Rees.TangyFruitMapper
{
    internal static class StringExtension
    {
        // Convert the string to camel case.
        public static string ConvertPascalCaseToCamelCase(this string pascalCaseString)
        {
            // If there are 0 or 1 characters, just return the string.
            if (pascalCaseString == null || pascalCaseString.Length < 2)
                return pascalCaseString;

            return pascalCaseString.Substring(0, 1).ToLower() + pascalCaseString.Substring(1);
        }
    }
}