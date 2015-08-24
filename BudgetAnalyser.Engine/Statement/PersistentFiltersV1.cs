using System;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    public class PersistentFiltersV1 : IPersistent
    {
        public BankAccount.Account Account { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int LoadSequence => 50;
    }
}