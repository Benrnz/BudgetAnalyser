using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    internal class DateTimeDescendingOrder : IComparer<DateTime>
    {
        public int Compare(DateTime x, DateTime y)
        {
            if (x < y)
            {
                return 1;
            }
            return x > y ? -1 : 0;
        }
    }
}
