using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    [AutoRegisterWithIoC(SingleInstance = true, RegisterAs = typeof(BasicMapper<LedgerBookDto, LedgerBook>))]
    public class DtoToLedgerBookMapper : BasicMapper<LedgerBookDto, LedgerBook>
    {
        private static readonly Dictionary<string, LedgerColumn> CachedLedgers = new Dictionary<string, LedgerColumn>();
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly ILogger logger;

        public DtoToLedgerBookMapper(
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

        public override LedgerBook Map([NotNull] LedgerBookDto source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var book = new LedgerBook(source.Name, source.Modified, source.FileName, this.logger);
            book.SetDatedEntries(MapLines(source.DatedEntries));

            var messages = new StringBuilder();
            if (!book.Validate(messages))
            {
                throw new FileFormatException(messages.ToString());
            }

            return book;
        }

        private IEnumerable<BankBalance> MapBankBalances(IEnumerable<BankBalanceDto> dataBalances)
        {
            return dataBalances.Select(d => new BankBalance(this.accountTypeRepository.GetOrCreateNew(d.Account), d.Balance));
        }

        private LedgerEntry MapEntry(LedgerEntryDto dataEntry, LedgerEntry previousEntry)
        {
            var entry = new LedgerEntry { LedgerColumn = MapLedger(dataEntry.BucketCode), Balance = previousEntry == null ? 0 : previousEntry.Balance };
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

        private List<LedgerEntryLine> MapLines(IEnumerable<LedgerEntryLineDto> dataLines)
        {
            List<LedgerEntryLineDto> localCopyOfLines = dataLines.Reverse().ToList(); // Now it is in ascending order starting at oldest date first.
            LedgerEntryLine previousLine = null;
            var listOfLines = new List<LedgerEntryLine>();
            foreach (LedgerEntryLineDto line in localCopyOfLines)
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
                    foreach (LedgerEntryDto entry in line.Entries)
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

        private List<LedgerTransaction> MapTransactions(IEnumerable<LedgerTransactionDto> dataTransactions)
        {
            var list = new List<LedgerTransaction>();
            foreach (LedgerTransactionDto dataTransaction in dataTransactions)
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
                transaction.BankAccount = this.accountTypeRepository.GetOrCreateNew(dataTransaction.AccountType)
                                          ?? this.accountTypeRepository.GetOrCreateNew(AccountTypeRepositoryConstants.Cheque);
                list.Add(transaction);
            }

            return list;
        }
    }
}