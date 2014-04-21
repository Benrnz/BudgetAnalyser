namespace BudgetAnalyser.Engine.Statement
{
    public interface IVersionedStatementModelRepository
    {
        bool IsValidFile(string fileName);

        StatementModel Load(string fileName);

        void Save(StatementModel model, string fileName);
    }
}