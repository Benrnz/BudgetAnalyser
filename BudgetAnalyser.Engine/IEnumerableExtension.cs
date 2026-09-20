namespace BudgetAnalyser.Engine;

/// <summary>
///     An extension class for <see cref="IEnumerable{T}" />
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    ///     Returns true if there are no elements in the collection. This is the logical opposite to Any()
    /// </summary>
    public static bool None<T>(this IEnumerable<T> instance)
    {
        return !instance.Any();
    }
}
