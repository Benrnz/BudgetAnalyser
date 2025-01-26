using System.Security.Cryptography;

namespace BudgetAnalyser.Engine;

/// <summary>
///     A generator that will produce unique(ish) reference numbers
/// </summary>
public static class ReferenceNumberGenerator
{
    private static readonly char[] DisallowedChars = ['\\', '{', '}', '[', ']', '^', '=', '/', ';', '.', ',', '-', '+'];

    /// <summary>
    ///     Create a small concise reference number that's 8 characters long.
    /// </summary>
    public static string IssueTransactionReferenceNumber()
    {
        var finalReference = new char[8];
        var acceptedChars = 0;
        var rng = RandomNumberGenerator.Create();
        do
        {
            var randomBytes = new byte[24];
            rng.GetNonZeroBytes(randomBytes);
            var base64CharArray = new char[32];
            Convert.ToBase64CharArray(randomBytes, 0, randomBytes.Length, base64CharArray, 0);

            for (var index = 0; index < 24; index++)
            {
                if (DisallowedChars.Contains(base64CharArray[index]))
                {
                    continue;
                }

                finalReference[acceptedChars++] = base64CharArray[index];
                if (acceptedChars >= 8)
                {
                    break;
                }
            }
        } while (acceptedChars < 8);

        return new string(finalReference);
    }
}
