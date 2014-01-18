using System;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Filtering
{
    public class FilterStateV1 
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public AccountType AccountType { get; set; }
    }
}
