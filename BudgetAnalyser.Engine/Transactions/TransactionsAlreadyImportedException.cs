namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     An exception to report that an attempt was made to import the same exact bank export twice.
/// </summary>
/// <seealso cref="System.Exception" />
public class TransactionsAlreadyImportedException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TransactionsAlreadyImportedException" /> class.
    /// </summary>
    public TransactionsAlreadyImportedException()
    {
    }
}
