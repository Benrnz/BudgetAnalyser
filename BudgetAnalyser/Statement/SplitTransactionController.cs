using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.ShellDialog;
using Rees.Wpf;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class SplitTransactionController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private readonly IBudgetBucketRepository bucketRepo;
        private Guid dialogCorrelationId;
        private string doNotUseInvalidMessage;
        private decimal doNotUseSplinterAmount1;
        private decimal doNotUseSplinterAmount2;

        public SplitTransactionController([NotNull] UiContext uiContext, [NotNull] IBudgetBucketRepository bucketRepo)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (bucketRepo == null)
            {
                throw new ArgumentNullException("bucketRepo");
            }

            this.bucketRepo = bucketRepo;
            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public string ActionButtonToolTip
        {
            get { return "Save Split Transactions."; }
        }

        public bool CanExecuteCancelButton
        {
            get { return true; }
        }

        public bool CanExecuteOkButton
        {
            get { return false; }
        }

        public bool CanExecuteSaveButton
        {
            get { return Valid; }
        }

        public string CloseButtonToolTip
        {
            get { return "Cancel."; }
        }

        public string InvalidMessage
        {
            get { return this.doNotUseInvalidMessage; }
            private set
            {
                this.doNotUseInvalidMessage = value;
                RaisePropertyChanged();
            }
        }

        public Transaction OriginalTransaction { get; private set; }

        public decimal SplinterAmount1
        {
            get { return this.doNotUseSplinterAmount1; }
            set
            {
                this.doNotUseSplinterAmount1 = value;
                this.doNotUseSplinterAmount2 = OriginalTransaction.Amount - value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => SplinterAmount2);
                RaisePropertyChanged(() => TotalAmount);
                RaisePropertyChanged(() => Valid);
            }
        }

        public decimal SplinterAmount2
        {
            get { return this.doNotUseSplinterAmount2; }
            set
            {
                this.doNotUseSplinterAmount2 = value;
                this.doNotUseSplinterAmount1 = OriginalTransaction.Amount - value;
                RaisePropertyChanged(() => SplinterAmount1);
                RaisePropertyChanged();
                RaisePropertyChanged(() => TotalAmount);
                RaisePropertyChanged(() => Valid);
            }
        }

        public decimal TotalAmount
        {
            get { return SplinterAmount1 + SplinterAmount2; }
        }

        public bool Valid
        {
            get
            {
                if (SplinterAmount1 == 0)
                {
                    InvalidMessage = "Amount 1 cannot be zero.";
                    return false;
                }

                if (SplinterAmount2 == 0)
                {
                    InvalidMessage = "Amount 2 cannot be zero.";
                    return false;
                }

                if (SplinterAmount1 + SplinterAmount2 != OriginalTransaction.Amount)
                {
                    InvalidMessage = string.Format(CultureInfo.CurrentCulture, "The two amounts do not add up to {0:C}", OriginalTransaction.Amount);
                    return false;
                }

                return true;
            }
        }

        public IEnumerable<BudgetBucket> BudgetBuckets { get; private set; }

        public BudgetBucket SplinterBucket1 { get; set; }

        public BudgetBucket SplinterBucket2 { get; set; }

        public void ShowDialog(Transaction originalTransaction, Guid correlationId)
        {
            BudgetBuckets = bucketRepo.Buckets;
            this.dialogCorrelationId = correlationId;
            OriginalTransaction = originalTransaction;
            SplinterAmount1 = OriginalTransaction.Amount;
            SplinterAmount2 = 0M;
            SplinterBucket2 = SplinterBucket1 = OriginalTransaction.BudgetBucket;

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.SaveCancel)
            {
                CorrelationId = correlationId,
                Title = "Split Transaction",
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            // StatementController processes the request to add the two new transactions.
            this.dialogCorrelationId = Guid.Empty;
            OriginalTransaction = null;
        }
    }
}