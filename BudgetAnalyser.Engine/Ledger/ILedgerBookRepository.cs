namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    /// An interface to provide access to retreive, store, and create <see cref="LedgerBook"/>s.
    /// </summary>
    public interface ILedgerBookRepository
    {
        LedgerBook CreateNew(string name, string fileName);
        bool Exists(string fileName);
        LedgerBook Load(string fileName);

        void Save(LedgerBook book);
        void Save(LedgerBook book, string fileName);
    }
}