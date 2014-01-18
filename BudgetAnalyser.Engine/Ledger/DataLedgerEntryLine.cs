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

        public List<DataLedgerTransaction> BankBalanceAdjustments { get; set; }

        public DateTime Date { get; set; }

        public List<DataLedgerEntry> Entries { get; set; }

        public string Remarks { get; set; }
    }
}