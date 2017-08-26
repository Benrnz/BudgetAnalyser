using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger.Reconciliation
{
    /// <summary>
    ///     An overdrawn surplus balance is not valid, and indicates that one or more ledger buckets have been overdrawn.  A
    ///     transfer probably needs to be manually done by the user.
    /// </summary>
    [AutoRegisterWithIoC]
    internal class ReconciliationBehaviourOverdrawnSurplus : IReconciliationBehaviour
    {
        public LedgerEntryLine NewReconLine { get; private set; }

        public IList<ToDoTask> TodoTasks { get; private set; }

        public void Dispose()
        {
            NewReconLine = null;
            TodoTasks = null;
        }

        public void Initialise(params object[] anyParameters)
        {
            foreach (var argument in anyParameters)
            {
                TodoTasks = TodoTasks ?? argument as IList<ToDoTask>;
                NewReconLine = NewReconLine ?? argument as LedgerEntryLine;
            }

            if (TodoTasks == null)
            {
                throw new ArgumentNullException(nameof(TodoTasks));
            }

            if (NewReconLine == null)
            {
                throw new ArgumentNullException(nameof(NewReconLine));
            }
        }

        public void ApplyBehaviour()
        {
            CreateToDoForAnyOverdrawnSurplusBalance();
        }

        private void CreateToDoForAnyOverdrawnSurplusBalance()
        {
            foreach (var surplusBalance in NewReconLine.SurplusBalances.Where(s => s.Balance < 0))
            {
                TodoTasks.Add(
                              new ToDoTask(
                                           string.Format(CultureInfo.CurrentCulture,
                                                         "{0} has a negative surplus balance {1}, there must be one or more transfers to action.",
                                                         surplusBalance.Account,
                                                         surplusBalance.Balance),
                                           true));
            }
        }
    }
}