using System;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    public class EditingTransactionController : ControllerBase
    {
        public EditingTransactionController(UiContext uiContext)
        {
            MessengerInstance = uiContext.Messenger;
        }

        private Transaction doNotUseTransaction;

        public Transaction Transaction
        {
            get { return this.doNotUseTransaction; }
            set
            {
                if (value == null)
                {
                    OriginalHash = 0;
                }
                else
                {
                    OriginalHash = value.GetEqualityHashCode();
                }

                this.doNotUseTransaction = value;
            }
        }

        public int OriginalHash { get; private set; }

        public bool HasChanged
        {
            get { return OriginalHash != Transaction.GetEqualityHashCode(); }
        }

        public void ShowDialog(Transaction transaction, Guid correlationId)
        {
            Transaction = transaction;
            MessengerInstance.Send(
                new ShellDialogRequestMessage(
                    BudgetAnalyserFeature.Transactions,
                    this,
                    ShellDialogType.Ok)
                {
                    CorrelationId = correlationId,
                    Title = "Edit Transaction",
                });
        }
    }
}
