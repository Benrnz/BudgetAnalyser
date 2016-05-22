using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     An object to encapsulate all necessary data to perform a transfer operation in a <see cref="LedgerEntry" />.
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class TransferFundsCommand : INotifyPropertyChanged
    {
        private string doNotUseAutoMatchingReference;
        private bool doNotUseBankTransferRequired;
        private LedgerBucket doNotUseFromLedger;
        private LedgerBucket doNotUseToLedger;

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     Gets or sets the automatic matching reference.
        /// </summary>
        /// <value>
        ///     The automatic matching reference.
        /// </value>
        public string AutoMatchingReference
        {
            get { return this.doNotUseAutoMatchingReference; }
            set
            {
                this.doNotUseAutoMatchingReference = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether a bank transfer is required.
        ///     Used to highlight to the user in the UI that a bank transfer needs to be performed for this transfer to be
        ///     complete.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the source ledger to transfer from.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the transfer narrative. This will be used on both transactions.
        /// </summary>
        public string Narrative { get; set; }

        /// <summary>
        ///     Gets or sets the destination ledger to transfer into.
        /// </summary>
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

        /// <summary>
        ///     Gets or sets the transfer amount.
        /// </summary>
        public decimal TransferAmount { get; set; }

        /// <summary>
        ///     Returns true if the transfer is valid.
        /// </summary>
        public bool IsValid()
        {
            var valid = Narrative.IsSomething()
                        && FromLedger != null
                        && ToLedger != null
                        && FromLedger != ToLedger
                        && TransferAmount > 0.0001M;
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

        /// <summary>
        ///     Called when a property has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
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