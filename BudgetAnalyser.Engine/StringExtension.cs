﻿namespace BudgetAnalyser.Engine;

/// <summary>
///     An extension class for string.
/// </summary>
public static class StringExtension
{
    private static readonly char[] Vowels = ['a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U'];

    /// <summary>
    ///     Returns the appropriate 'An' or 'A' prefix for the given string.  The given string is not included in the return text.
    /// </summary>
    /// <param name="instance">The string to determine the prefix for.</param>
    /// <param name="properCase">A boolean to indicate if the prefix should be proper-cased or not.</param>
    public static string AnOrA(this string instance, bool properCase = false)
    {
        if (string.IsNullOrWhiteSpace(instance))
        {
            return instance;
        }

        var useAn = Vowels.Contains(instance.ToCharArray(0, 1)[0]);

        if (properCase && useAn)
        {
            return "An";
        }

        if (properCase)
        {
            return "A";
        }

        return useAn ? "an" : "a";
    }

    /// <summary>
    ///     Returns true if the string is null, whitespace or empty. Equivalent to IsNullOrWhiteSpace, but reads better.
    ///     The reason for this function is to provide a more readable way to check if a string is nothing or whitespace.
    /// </summary>
    public static bool IsNothing(this string? instance)
    {
        return string.IsNullOrWhiteSpace(instance);
    }

    /// <summary>
    ///     Returns true if the string is not null, not whitespace, and not empty. The direct opposite of <see cref="IsNothing" />.
    ///     The reason for this function is to provide a more readable way to check if a string is not nothing or whitespace.
    /// </summary>
    public static bool IsSomething(this string? instance)
    {
        return !string.IsNullOrWhiteSpace(instance);
    }

    /// <summary>
    ///     An extension to the <see cref="string.Split(char[])" /> method. This method splits at new line characters, trims off trailing whitespace, and returns the specified number of lines.
    /// </summary>
    public static string[] SplitLines(this string instance, int numberOfLines = 0)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (numberOfLines < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfLines), "Number of Lines must be a positive integer.");
        }

        var split = instance.Split('\r', '\n');
        var query = split.Where(l => l.Length > 0);
        if (numberOfLines > 0)
        {
            query = query.Take(numberOfLines);
        }

        return query.ToArray();
    }

    /// <summary>
    ///     Truncates the right of a string to the specified length, but only if it exceeds that length. Optionally the
    ///     returned string can include ellipses.
    /// </summary>
    public static string Truncate(this string? instance, int truncateToLength, bool useEllipses = false)
    {
        if (string.IsNullOrWhiteSpace(instance))
        {
            return string.Empty;
        }

        if (instance.Length <= truncateToLength)
        {
            return instance;
        }

        return useEllipses ? instance.Substring(0, truncateToLength - 1) + "…" : instance.Substring(0, truncateToLength);
    }

    /// <summary>
    ///     Truncates the left of a string to the specified length, but only if it exceeds that length. Optionally the returned string can include ellipses.
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

        return useEllipses
            ? "…" + instance.Substring(instance.Length - truncateToLength + 1, truncateToLength - 1)
            : instance.Substring(instance.Length - truncateToLength, truncateToLength);
    }
}
