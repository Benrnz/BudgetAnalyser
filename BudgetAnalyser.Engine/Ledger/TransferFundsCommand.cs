using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    public class TransferFundsCommand : INotifyPropertyChanged
    {
        private string doNotUseAutoMatchingReference;
        private bool doNotUseBankTransferRequired;
        private LedgerBucket doNotUseFromLedger;
        private LedgerBucket doNotUseToLedger;
        public event PropertyChangedEventHandler PropertyChanged;

        public string AutoMatchingReference
        {
            get { return this.doNotUseAutoMatchingReference; }
            set
            {
                this.doNotUseAutoMatchingReference = value;
                OnPropertyChanged();
            }
        }

        public bool BankTransferRequired
        {
            get { return this.doNotUseBankTransferRequired; }
            set
            {
                this.doNotUseBankTransferRequired = value;
                OnPropertyChanged();
                if (BankTransferRequired && AutoMatchingReference.IsNothing())
                {
                    AutoMatchingReference = ReferenceNumberGenerator.IssueTransactionReferenceNumber();
                }
            }
        }

        public LedgerBucket FromLedger
        {
            get { return this.doNotUseFromLedger; }
            set
            {
                this.doNotUseFromLedger = value;
                OnPropertyChanged();
                SetBankTransferRequired();
            }
        }

        public string Narrative { get; set; }

        public LedgerBucket ToLedger
        {
            get { return this.doNotUseToLedger; }
            set
            {
                this.doNotUseToLedger = value;
                OnPropertyChanged();
                SetBankTransferRequired();
            }
        }

        public decimal TransferAmount { get; set; }

        public bool IsValid()
        {
            bool valid = FromLedger != null
                         && ToLedger != null
                         && FromLedger != ToLedger
                         && TransferAmount > 0;
            if (!valid)
            {
                return false;
            }

            if (BankTransferRequired)
            {
                valid = AutoMatchingReference.IsSomething();
            }

            if (FromLedger.BudgetBucket is SurplusBucket
                && ToLedger.BudgetBucket is SurplusBucket
                && FromLedger.StoredInAccount == ToLedger.StoredInAccount)
            {
                valid = false;
            }

            return valid;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetBankTransferRequired()
        {
            if (FromLedger != null && ToLedger != null)
            {
                BankTransferRequired = FromLedger.StoredInAccount != ToLedger.StoredInAccount;
            }
        }
    }
}