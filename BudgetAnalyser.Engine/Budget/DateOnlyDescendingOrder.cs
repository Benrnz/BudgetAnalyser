namespace BudgetAnalyser.Engine.Budget;

internal class DateOnlyDescendingOrder : IComparer<DateOnly>
{
    public int Compare(DateOnly x, DateOnly y)
    {
        if (x < y)
        {
            return 1;
        }

        return x > y ? -1 : 0;
    }
}
