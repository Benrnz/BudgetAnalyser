using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Ledger;

/// <summary>
///     An object to encapsulate all necessary data to perform a transfer operation in a <see cref="LedgerEntry" />.
/// </summary>
/// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
public class TransferFundsCommand : INotifyPropertyChanged
{
    private string doNotUseAutoMatchingReference = string.Empty;
    private bool doNotUseBankTransferRequired;
    private LedgerBucket? doNotUseFromLedger;
    private string doNotUseNarrative = string.Empty;
    private LedgerBucket? doNotUseToLedger;
    private decimal doNotUseTransferAmount;
    private bool isValid;

    /// <summary>
    ///     Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///     Gets or sets the automatic matching reference.
    /// </summary>
    /// <value>
    ///     The automatic matching reference.
    /// </value>
    public string AutoMatchingReference
    {
        get => this.doNotUseAutoMatchingReference;
        set
        {
            if (value == this.doNotUseAutoMatchingReference)
            {
                return;
            }

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
        get => this.doNotUseBankTransferRequired;
        set
        {
            if (value == this.doNotUseBankTransferRequired)
            {
                return;
            }

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
    public LedgerBucket? FromLedger
    {
        get => this.doNotUseFromLedger;
        set
        {
            if (Equals(value, this.doNotUseFromLedger))
            {
                return;
            }

            this.doNotUseFromLedger = value;
            OnPropertyChanged();
            SetBankTransferRequired();
            if (IsValid != this.isValid)
            {
                this.isValid = !this.isValid;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>
    ///     Returns true if the transfer is valid.
    /// </summary>
    public bool IsValid
    {
        get
        {
            var valid = Narrative.IsSomething()
                        && FromLedger is not null
                        && ToLedger is not null
                        && FromLedger != ToLedger
                        && TransferAmount >= 0.01M;
            if (!valid)
            {
                return false;
            }

            if (BankTransferRequired)
            {
                valid = AutoMatchingReference.IsSomething();
            }

            if (FromLedger!.BudgetBucket is SurplusBucket
                && ToLedger!.BudgetBucket is SurplusBucket
                && FromLedger.StoredInAccount == ToLedger.StoredInAccount)
            {
                valid = false;
            }

            return valid;
        }
    }

    /// <summary>
    ///     Gets or sets the transfer narrative. This will be used on both transactions.
    /// </summary>
    public string Narrative
    {
        get => this.doNotUseNarrative;
        set
        {
            if (value == this.doNotUseNarrative)
            {
                return;
            }

            this.doNotUseNarrative = value;
            OnPropertyChanged();
            if (IsValid != this.isValid)
            {
                this.isValid = !this.isValid;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>
    ///     Gets or sets the destination ledger to transfer into.
    /// </summary>
    public LedgerBucket? ToLedger
    {
        get => this.doNotUseToLedger;
        set
        {
            if (Equals(value, this.doNotUseToLedger))
            {
                return;
            }

            this.doNotUseToLedger = value;
            OnPropertyChanged();
            SetBankTransferRequired();
            if (IsValid != this.isValid)
            {
                this.isValid = !this.isValid;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>
    ///     Gets or sets the transfer amount.
    /// </summary>
    public decimal TransferAmount
    {
        get => this.doNotUseTransferAmount;
        set
        {
            if (value == this.doNotUseTransferAmount)
            {
                return;
            }

            this.doNotUseTransferAmount = value;
            OnPropertyChanged();
            if (IsValid != this.isValid)
            {
                this.isValid = !this.isValid;
                OnPropertyChanged(nameof(IsValid));
            }
        }
    }

    /// <summary>
    ///     Called when a property has changed.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetBankTransferRequired()
    {
        if (FromLedger is not null && ToLedger is not null)
        {
            BankTransferRequired = FromLedger.StoredInAccount != ToLedger.StoredInAccount;
        }
    }
}
