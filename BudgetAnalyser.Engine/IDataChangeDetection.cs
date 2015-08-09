namespace BudgetAnalyser.Engine
{
    public interface IDataChangeDetection
    {
        long SignificantDataChangeHash();
    }
}