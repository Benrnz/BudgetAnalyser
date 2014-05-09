using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger
{
    public class DataLedgerEntryLine
    {
        public DataLedgerEntryLine()
        {
            Entries = new List<DataLedgerEntry>();
            BankBalanceAdjustments = new List<DataLedgerTransaction>();
        }

        public decimal BankBalance { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerTransaction> BankBalanceAdjustments { get; set; }

        public DateTime Date { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerEntry> Entries { get; set; }

        public string Remarks { get; set; }
    }
}