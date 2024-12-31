namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     Describes an interface that will allow consumers to detect data state changes compared with other instances of the
    ///     same type.
    /// </summary>
    public interface IDataChangeDetection
    {
        /// <summary>
        ///     Calcuates a hash that represents a data state for the current instance.  When the data state changes the hash will
        ///     change.
        /// </summary>
        long SignificantDataChangeHash();
    }
}
