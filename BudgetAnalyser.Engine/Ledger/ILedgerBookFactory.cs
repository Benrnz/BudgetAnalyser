namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A factory to create new instances of a <see cref="LedgerBook" />.
    ///     Used by persistence and when the user creates a new one.
    /// </summary>
    internal interface ILedgerBookFactory
    {
        LedgerBook CreateNew();
    }
}