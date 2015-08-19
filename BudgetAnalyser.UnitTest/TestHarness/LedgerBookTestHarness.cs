using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class LedgerBookTestHarness : LedgerBook
    {
        /// <summary>
        /// Constructs a new instance of the <see cref="LedgerBook"/> class.  The Persistence system calls this constructor, not the IoC system. 
        /// </summary>
        public LedgerBookTestHarness([NotNull] IReconciliationBuilder reconciliationBuilder) : base(new FakeLogger(), reconciliationBuilder)
        {
        }

        internal override LedgerEntryLine Reconcile(
            DateTime reconciliationStartDate,
            IEnumerable<BankBalance> currentBankBalances,
            BudgetModel budget,
            ToDoCollection toDoList = null,
            StatementModel statement = null,
            bool ignoreWarnings = false)
        {
            return null;
        }
    }
}
