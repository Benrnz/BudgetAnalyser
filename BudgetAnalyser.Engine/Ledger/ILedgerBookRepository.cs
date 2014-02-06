namespace BudgetAnalyser.Engine.Ledger
{
    public interface ILedgerBookRepository
    {
        bool Exists(string fileName);
        LedgerBook Load(string fileName);

        void Save(LedgerBook book);
        void Save(LedgerBook book, string fileName);
    }
}