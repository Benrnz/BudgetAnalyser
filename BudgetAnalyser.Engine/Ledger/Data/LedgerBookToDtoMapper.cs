using System;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBook, LedgerBookDto>))]
    public class LedgerBookToDtoMapper : BasicMapper<LedgerBook, LedgerBookDto>, ILedgerDomainToDataMapper
    {
        public override LedgerBookDto Map([NotNull] LedgerBook domainBook)
        {
            if (domainBook == null)
            {
                throw new ArgumentNullException("domainBook");
            }

            var dataBook = new LedgerBookDto
            {
                DatedEntries = domainBook.DatedEntries.Select(MapLine).ToList(),
                FileName = domainBook.FileName,
                Modified = domainBook.Modified,
                Name = domainBook.Name,
            };

            return dataBook;
        }

        private BankBalanceDto MapBankBalance(BankBalance bankBalance)
        {
            return new BankBalanceDto { Account = bankBalance.Account.Name, Balance = bankBalance.Balance };
        }

        private LedgerEntryDto MapEntry(LedgerEntry entry)
        {
            var dataEntry = new LedgerEntryDto
            {
                Balance = entry.Balance,
                BucketCode = entry.LedgerColumn.BudgetBucket.Code,
                Transactions = entry.Transactions.Select(MapTransaction).ToList()
            };

            return dataEntry;
        }

        private LedgerEntryLineDto MapLine(LedgerEntryLine line)
        {
            var dataLine = new LedgerEntryLineDto
            {
                BankBalance = line.TotalBankBalance,
                BankBalances = line.BankBalances.Select(MapBankBalance).ToList(),
                BankBalanceAdjustments = line.BankBalanceAdjustments.Select(MapTransaction).ToList(),
                Date = line.Date,
                Entries = line.Entries.Select(MapEntry).ToList(),
                Remarks = line.Remarks,
            };

            return dataLine;
        }

        private LedgerTransactionDto MapTransaction(LedgerTransaction transaction)
        {
            var dataTransaction = new LedgerTransactionDto
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