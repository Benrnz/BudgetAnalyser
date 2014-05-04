using System;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerDomainToDataMapper : ILedgerDomainToDataMapper
    {
        public DataLedgerBook Map([NotNull] LedgerBook domainBook)
        {
            if (domainBook == null)
            {
                throw new ArgumentNullException("domainBook");
            }

            var dataBook = new DataLedgerBook
            {
                DatedEntries = domainBook.DatedEntries.Select(MapLine).ToList(),
                FileName = domainBook.FileName,
                Modified = domainBook.Modified,
                Name = domainBook.Name,
            };

            return dataBook;
        }

        private DataLedgerEntry MapEntry(LedgerEntry entry)
        {
            var dataEntry = new DataLedgerEntry
            {
                Balance = entry.Balance,
                BucketCode = entry.Ledger.BudgetBucket.Code,
                Transactions = entry.Transactions.Select(MapTransaction).ToList()
            };

            return dataEntry;
        }

        private DataLedgerEntryLine MapLine(LedgerEntryLine line)
        {
            var dataLine = new DataLedgerEntryLine
            {
                BankBalance = line.BankBalance,
                BankBalanceAdjustments = line.BankBalanceAdjustments.Select(MapTransaction).ToList(),
                Date = line.Date,
                Entries = line.Entries.Select(MapEntry).ToList(),
                Remarks = line.Remarks,
            };

            return dataLine;
        }

        private DataLedgerTransaction MapTransaction(LedgerTransaction transaction)
        {
            var dataTransaction = new DataLedgerTransaction
            {
                Credit = transaction.Credit,
                Debit = transaction.Debit,
                Id = transaction.Id,
                Narrative = transaction.Narrative,
                TransactionType = transaction.GetType().FullName,
            };

            return dataTransaction;
        }
    }
}