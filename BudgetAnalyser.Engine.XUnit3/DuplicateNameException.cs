namespace BudgetAnalyser.Engine.XUnit;

internal class DuplicateNameException : Exception
{
    public DuplicateNameException(string message) : base(message)
    {
    }
}
