// TODO Move to statement

namespace BudgetAnalyser.Engine.Statement
{
    public interface IVersionedStatementModelImporter
    {
        bool IsValidFile(string fileName);

        StatementModel Load(string fileName);

        void Save(StatementModel model, string fileName);
    }
}