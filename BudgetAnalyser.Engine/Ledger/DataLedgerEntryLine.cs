using System;
using System.Collections.Generic;

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerTransaction> BankBalanceAdjustments { get; set; }

        public DateTime Date { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Required for serialisation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required for serialisation")]
        public List<DataLedgerEntry> Entries { get; set; }

        public string Remarks { get; set; }
    }
}