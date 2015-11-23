﻿using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class LedgerBookTestHarness : LedgerBook
    {
        /// <summary>
        ///     Constructs a new instance of the <see cref="LedgerBook" /> class.  The Persistence system calls this constructor,
        ///     not the IoC system.
        /// </summary>
        public LedgerBookTestHarness([NotNull] IReconciliationBuilder reconciliationBuilder)
            : base(reconciliationBuilder)
        {
        }

        public Func<ReconciliationResult> ReconcileOverride { get; set; }

        internal override ReconciliationResult Reconcile(DateTime reconciliationDate, BudgetModel budget, StatementModel statement, params BankBalance[] currentBankBalances)
        {
            return ReconcileOverride?.Invoke();
        }
    }
}