using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Budget
{
    public class DataBudgetCollection
    {
        public string FileName { get; set; }

        public List<DataBudgetModel> Budgets { get; set; }
    }
}
