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
        private readonly IBudgetBucketRepository bucketRepository;
        private Guid dialogCorrelationId;
        private string doNotUseAutoMatchingReference;
        private bool doNotUseBankTransferRequired;
        private Account doNotUseFromAccount;
        private LedgerForTransferFunds doNotUseSelectedFromLedgerBucket;
        private LedgerForTransferFunds doNotUseSelectedToLedgerBucket;
        private Account doNotUseToAccount;

        private BudgetBucket fromLedger;
        private List<LedgerBucket> ledgers;
        private BudgetBucket toLedger;

        public TransferFundsController([NotNull] IMessenger messenger, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }
            this.bucketRepository = bucketRepository;
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

        public IEnumerable<LedgerForTransferFunds> LedgerBuckets { get; private set; }

        public string Narrative { get; set; }

        public LedgerForTransferFunds SelectedFromLedgerBucket
        {
            get { return this.doNotUseSelectedFromLedgerBucket; }
            set
            {
                this.doNotUseSelectedFromLedgerBucket = value;
                RaisePropertyChanged();
                FromAccount = SelectedFromLedgerBucket?.Account;
                this.fromLedger = SyncLedger(SelectedFromLedgerBucket);
            }
        }

        public LedgerForTransferFunds SelectedToLedgerBucket
        {
            get { return this.doNotUseSelectedToLedgerBucket; }
            set
            {
                this.doNotUseSelectedToLedgerBucket = value;
                RaisePropertyChanged();
                ToAccount = SelectedToLedgerBucket?.Account;
                this.toLedger = SyncLedger(SelectedToLedgerBucket);
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

        public void ShowDialog(IEnumerable<LedgerBucket> ledgerBuckets, IEnumerable<Account> ledgerAccounts)
        {
            Reset();
            this.ledgers = ledgerBuckets.ToList();
            List<LedgerForTransferFunds> ledgersList =
                this.ledgers.Select(l => new LedgerForTransferFunds { Key = l.BudgetBucket.Code, Account = l.StoredInAccount, DisplayName = l.BudgetBucket.ToString() }).ToList();
            foreach (Account account in ledgerAccounts)
            {
                ledgersList.Insert(0, new LedgerForTransferFunds { Key = SurplusBucket.SurplusCode, Account = account, DisplayName = $"Surplus in {account.Name}" });
            }

            LedgerBuckets = ledgersList;
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
                if (this.fromLedger is SurplusBucket && this.toLedger is SurplusBucket)
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

        private BudgetBucket SyncLedger(LedgerForTransferFunds selectedBucket)
        {
            if (selectedBucket == null) return null;
            if (selectedBucket.Key == SurplusBucket.SurplusCode)
            {
                return this.bucketRepository.SurplusBucket;
            }

            return this.ledgers.Single(l => l.BudgetBucket.Code == selectedBucket.Key).BudgetBucket;
        }
    }
}