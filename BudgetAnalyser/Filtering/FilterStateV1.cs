using System;
using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Filtering
{
    public class FilterStateV1
    {
        public AccountType AccountType { get; set; }
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}