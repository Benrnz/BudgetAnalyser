using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    ///     A Dto for <see cref="LedgerEntryLine" />
    /// </summary>
    public class LedgerEntryLineDto
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LedgerEntryLineDto" /> class.
        /// </summary>
        public LedgerEntryLineDto()
        {
            Entries = new List<LedgerEntryDto>();
            BankBalanceAdjustments = new List<LedgerTransactionDto>();
            BankBalances = new List<BankBalanceDto>();
        }

        /// <summary>
        ///     Total bank balance, ie sum of all <see cref="BankBalances" />
        ///     Doesn't include balance adjustments.
        /// </summary>
        public decimal BankBalance { get; set; }

        /// <summary>
        ///     Gets or sets the bank balance adjustments.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<LedgerTransactionDto> BankBalanceAdjustments { get; set; }

        /// <summary>
        ///     Gets or sets the bank balances.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<BankBalanceDto> BankBalances { get; set; }

        /// <summary>
        ///     Gets or sets the date of the reconciliation.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        ///     Gets or sets the entries.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Required for serialisation")]
        public List<LedgerEntryDto> Entries { get; set; }

        /// <summary>
        ///     Gets or sets the remarks.
        /// </summary>
        public string Remarks { get; set; }
    }
}