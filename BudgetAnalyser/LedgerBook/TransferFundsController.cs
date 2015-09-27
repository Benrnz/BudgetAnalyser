using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class TransferFundsController : ControllerBase, IShellDialogToolTips, IShellDialogInteractivity
    {
        private Guid dialogCorrelationId;
        private string doNotUseAutoMatchingReference;
        private bool doNotUseBankTransferRequired;
        private Account doNotUseFromAccount;
        private LedgerBucket doNotUseSelectedFromLedgerBucket;
        private LedgerBucket doNotUseSelectedToLedgerBucket;
        private Account doNotUseToAccount;

        public TransferFundsController([NotNull] IMessenger messenger)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            MessengerInstance = messenger;
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogResponseReceived);
        }

        public string ActionButtonToolTip => "Save and action the transfer.";

        public string AutoMatchingReference
        {
            get { return this.doNotUseAutoMatchingReference; }
            private set
            {
                this.doNotUseAutoMatchingReference = value;
                RaisePropertyChanged();
            }
        }

        public bool BankTransferConfirmed { get; set; }

        public bool BankTransferRequired
        {
            get { return this.doNotUseBankTransferRequired; }
            private set
            {
                this.doNotUseBankTransferRequired = value;
                RaisePropertyChanged();
                if (BankTransferRequired && AutoMatchingReference.IsNothing())
                {
                    AutoMatchingReference = ReferenceNumberGenerator.IssueTransactionReferenceNumber();
                }
            }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton => true;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton => false;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton => IsOkToSave();

        public string CloseButtonToolTip => "Cancel the transfer.";

        public Account FromAccount
        {
            get { return this.doNotUseFromAccount; }
            set
            {
                this.doNotUseFromAccount = value;
                RaisePropertyChanged();
                AccountsChanged();
            }
        }

        public IEnumerable<LedgerBucket> LedgerBuckets { get; private set; }

        public string Narrative { get; set; }

        public LedgerBucket SelectedFromLedgerBucket
        {
            get { return this.doNotUseSelectedFromLedgerBucket; }
            set
            {
                this.doNotUseSelectedFromLedgerBucket = value;
                RaisePropertyChanged();
                FromAccount = SelectedFromLedgerBucket?.StoredInAccount;
            }
        }

        public LedgerBucket SelectedToLedgerBucket
        {
            get { return this.doNotUseSelectedToLedgerBucket; }
            set
            {
                this.doNotUseSelectedToLedgerBucket = value;
                RaisePropertyChanged();
                ToAccount = SelectedToLedgerBucket?.StoredInAccount;
            }
        }

        public Account ToAccount
        {
            get { return this.doNotUseToAccount; }
            set
            {
                this.doNotUseToAccount = value;
                RaisePropertyChanged();
                AccountsChanged();
            }
        }

        public decimal TransferAmount { get; set; }

        public void ShowDialog(IEnumerable<LedgerBucket> ledgerBuckets)
        {
            Reset();
            LedgerBuckets = ledgerBuckets.ToList();
            this.dialogCorrelationId = Guid.NewGuid();
            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.SaveCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Transfer Funds"
            };
            MessengerInstance.Send(dialogRequest);
        }

        private void AccountsChanged()
        {
            if (FromAccount == null || ToAccount == null)
            {
                return;
            }

            BankTransferRequired = FromAccount != ToAccount;
        }

        private bool IsOkToSave()
        {
            bool valid = SelectedFromLedgerBucket != null
                         && SelectedToLedgerBucket != null
                         && SelectedFromLedgerBucket != SelectedToLedgerBucket
                         && FromAccount != null
                         && ToAccount != null
                         && TransferAmount > 0;
            if (!valid)
            {
                return false;
            }

            if (BankTransferRequired)
            {
                valid = AutoMatchingReference.IsSomething()
                        && BankTransferConfirmed;
            }

            if (FromAccount == ToAccount)
            {
                if (SelectedFromLedgerBucket.BudgetBucket is SurplusBucket && SelectedToLedgerBucket.BudgetBucket is SurplusBucket)
                {
                    valid = false;
                }
            }

            return valid;
        }

        private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId))
            {
                return;
            }

            if (message.Response == ShellDialogButton.Cancel) return;

            // TODO do the transfer
            var transferCommand = new TransferFundsCommand
            {
                AutoMatchingReference = AutoMatchingReference,
                FromLedger = SelectedFromLedgerBucket,
                ToLedger = SelectedToLedgerBucket,
                Narrative = Narrative,
                TransferAmount = TransferAmount,
                BankTransferRequired = BankTransferRequired,
            };

            Reset();
        }

        private void Reset()
        {
            AutoMatchingReference = null;
            BankTransferRequired = false;
            FromAccount = null;
            ToAccount = null;
            Narrative = null;
            SelectedFromLedgerBucket = null;
            SelectedToLedgerBucket = null;
            TransferAmount = 0;
        }
    }
}