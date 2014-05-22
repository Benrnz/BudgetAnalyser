using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class LedgerDataToDomainMapper : ILedgerDataToDomainMapper
    {
        private static readonly Dictionary<string, LedgerColumn> CachedLedgers = new Dictionary<string, LedgerColumn>();
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly ILogger logger;

        public LedgerDataToDomainMapper(
            [NotNull] ILogger logger,
            [NotNull] IBudgetBucketRepository bucketRepository, 
            [NotNull] IAccountTypeRepository accountTypeRepository
            )
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }
            
            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.bucketRepository = bucketRepository;
            this.accountTypeRepository = accountTypeRepository;
            this.logger = logger;
        }

        public LedgerBook Map([NotNull] DataLedgerBook dataBook)
        {
            if (dataBook == null)
            {
                throw new ArgumentNullException("dataBook");
            }

            var book = new LedgerBook(dataBook.Name, dataBook.Modified, dataBook.FileName, this.logger);
            book.SetDatedEntries(MapLines(dataBook.DatedEntries));

            var messages = new StringBuilder();
            if (!book.Validate(messages))
            {
                throw new FileFormatException(messages.ToString());
            }

            return book;
        }

        private static List<LedgerTransaction> MapTransactions(IEnumerable<DataLedgerTransaction> dataTransactions)
        {
            var list = new List<LedgerTransaction>();
            foreach (DataLedgerTransaction dataTransaction in dataTransactions)
            {
                if (string.IsNullOrWhiteSpace(dataTransaction.TransactionType))
                {
                    throw new FileFormatException(string.Format(CultureInfo.CurrentCulture, "A null transaction type was encountered in transaction with narrative: {0} and amount {1:C}",
                        dataTransaction.Narrative,
                        dataTransaction.Credit - dataTransaction.Debit));
                }

                Type transactionType = Type.GetType(dataTransaction.TransactionType);
                if (transactionType == null)
                {
                    throw new FileFormatException(string.Format(CultureInfo.CurrentCulture, "Invalid transaction type was encountered in transaction with narrative: {0} and amount {1:C}",
                        dataTransaction.Narrative,
                        dataTransaction.Credit - dataTransaction.Debit));
                }

                var transaction = Activator.CreateInstance(transactionType, dataTransaction.Id) as LedgerTransaction;
                if (transaction == null)
                {
                    throw new FileFormatException("Invalid transaction type encountered: " + dataTransaction.TransactionType);
                }

                transaction.Credit = dataTransaction.Credit;
                transaction.Debit = dataTransaction.Debit;
                transaction.Narrative = dataTransaction.Narrative;
                list.Add(transaction);
            }

            return list;
        }

        private LedgerEntry MapEntry(DataLedgerEntry dataEntry, LedgerEntry previousEntry)
        {
            var entry = new LedgerEntry(MapLedger(dataEntry.BucketCode), previousEntry);
            entry.SetTransactions(MapTransactions(dataEntry.Transactions));
            return entry;
        }

        private LedgerColumn MapLedger(string budgetBucketCode)
        {
            if (CachedLedgers.ContainsKey(budgetBucketCode))
            {
                return CachedLedgers[budgetBucketCode];
            }

            var ledger = new LedgerColumn { BudgetBucket = this.bucketRepository.GetByCode(budgetBucketCode) };
            if (ledger.BudgetBucket == null)
            {
                throw new FileFormatException("Invalid Budget Bucket Code: " + budgetBucketCode);
            }

            CachedLedgers.Add(ledger.BudgetBucket.Code, ledger);
            return ledger;
        }

        private IEnumerable<BankBalance> MapBankBalances(IEnumerable<DataBankBalance> dataBalances)
        {
            return dataBalances.Select(d => new BankBalance { Account = this.accountTypeRepository.GetByKey(d.Account), Balance = d.Balance });
        }

        private List<LedgerEntryLine> MapLines(IEnumerable<DataLedgerEntryLine> dataLines)
        {
            List<DataLedgerEntryLine> localCopyOfLines = dataLines.Reverse().ToList(); // Now it is in ascending order starting at oldest date first.
            LedgerEntryLine previousLine = null;
            var listOfLines = new List<LedgerEntryLine>();
            foreach (DataLedgerEntryLine line in localCopyOfLines)
            {
                var domainLine = new LedgerEntryLine(line.Date, MapBankBalances(line.BankBalances), line.Remarks);

                domainLine.SetBalanceAdjustments(MapTransactions(line.BankBalanceAdjustments));
                if (previousLine == null)
                {
                    // No previous line because this is the first line in the ledger.
                    domainLine.SetEntries(line.Entries.Select(e => MapEntry(e, null)).ToList());
                }
                else
                {
                    var entries = new List<LedgerEntry>();
                    foreach (DataLedgerEntry entry in line.Entries)
                    {
                        LedgerEntry previousEntry = previousLine.Entries.FirstOrDefault(e => e.LedgerColumn.BudgetBucket.Code == entry.BucketCode);
                        entries.Add(MapEntry(entry, previousEntry));
                    }

                    domainLine.SetEntries(entries);
                }

                listOfLines.Add(domainLine);
                previousLine = domainLine;
            }

            return listOfLines.ToList();
        }
    }
}