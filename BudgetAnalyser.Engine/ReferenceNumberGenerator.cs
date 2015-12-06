using System;
using System.Text;

namespace BudgetAnalyser.Engine
{
    public static class ReferenceNumberGenerator
    {
        private static readonly string[] DisallowedChars = { "\\", "{", "}", "[", "]", "^", "=", "/", ";", ".", ",", "-", "+" };

        public static string IssueTransactionReferenceNumber()
        {
            var reference = new StringBuilder();
            do
            {
                reference.Append(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
                foreach (string disallowedChar in DisallowedChars)
                {
                    reference.Replace(disallowedChar, string.Empty);
                }
            } while (reference.Length < 8);
            return reference.ToString().Substring(0, 7);
        }
    }
}