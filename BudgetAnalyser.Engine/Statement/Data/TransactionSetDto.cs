using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.Statement.Data
{
    public class TransactionSetDto
    {
        public long Checksum { get; set; }

        public string FileName { get; set; }

        public DateTime LastImport { get; set; }

        public List<TransactionDto> Transactions { get; set; }

        public string VersionHash { get; set; }
    }
}