using System;
using System.Text;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     A generator that will produce unique(ish) reference numbers
    /// </summary>
    public static class ReferenceNumberGenerator
    {
        private static readonly string[] DisallowedChars =
        {
            "\\", "{", "}", "[", "]", "^", "=", "/", ";", ".", ",", "-",
            "+"
        };

        /// <summary>
        ///     Create a small concise reference number thats 8 characters long.
        /// </summary>
        public static string IssueTransactionReferenceNumber()
        {
            var reference = new StringBuilder();
            do
            {
                reference.Append(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
                foreach (var disallowedChar in DisallowedChars)
                {
                    reference.Replace(disallowedChar, string.Empty);
                }
            } while (reference.Length < 8);
            return reference.ToString().Substring(0, 7);
        }
    }
}