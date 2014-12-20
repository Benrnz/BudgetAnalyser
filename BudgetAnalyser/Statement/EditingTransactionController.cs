using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class EditingTransactionController : ControllerBase
    {
        private Transaction doNotUseTransaction;

        public EditingTransactionController([NotNull] UiContext uiContext)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            MessengerInstance = uiContext.Messenger;
        }

        public bool HasChanged
        {
            get { return OriginalHash != Transaction.GetEqualityHashCode(); }
        }

        public int OriginalHash { get; private set; }

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

        public void ShowDialog(Transaction transaction, Guid correlationId)
        {
            Transaction = transaction;
            MessengerInstance.Send(
                new ShellDialogRequestMessage(
                    BudgetAnalyserFeature.Transactions,
                    this,
                    ShellDialogType.SaveCancel)
                {
                    CorrelationId = correlationId,
                    Title = "Edit Transaction",
                });
        }
    }
}