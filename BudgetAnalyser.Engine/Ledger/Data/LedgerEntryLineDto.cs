using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    public class LedgerEntryLineDto
    {
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<LedgerTransactionDto> BankBalanceAdjustments { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<BankBalanceDto> BankBalances { get; set; }

        public DateTime Date { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<LedgerEntryDto> Entries { get; set; }

        public string Remarks { get; set; }
    }
}