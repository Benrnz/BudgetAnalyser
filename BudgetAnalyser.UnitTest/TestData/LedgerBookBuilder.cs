using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.TestData
{
    /// <summary>
    ///     Trialing a Fluent Builder pattern instead of a Object Mother pattern
    /// </summary>
    public class LedgerBookBuilder
    {
        private readonly List<LedgerBucket> ledgerBuckets = new List<LedgerBucket>();
        private readonly Dictionary<LedgerBucket, decimal> openingBalances = new Dictionary<LedgerBucket, decimal>();
        private readonly List<LedgerEntryLine> reconciliations = new List<LedgerEntryLine>();
        private bool lockWhenFinished = true;
        private IEnumerable<BankBalance> tempBankBalances;
        private DateTime tempReconDate;

        public IEnumerable<LedgerBucket> LedgerBuckets => this.ledgerBuckets;
        public DateTime Modified { get; set; } = new DateTime(2013, 12, 16);

        public string Name { get; set; } = "Test Data Book - built by LedgerBookBuilder";

        public IEnumerable<LedgerEntryLine> Reconciliations => this.reconciliations;
        public string StorageKey { get; set; } = "C:\\Folder\\book1.xml";

        public LedgerBook Build()
        {
            var book = new LedgerBook(new ReconciliationBuilder(new FakeLogger()))
            {
                Name = Name,
                Modified = Modified,
                StorageKey = StorageKey
            };

            book.SetReconciliations(this.reconciliations);

            return LedgerBookTestData.Finalise(book, this.lockWhenFinished);
        }

        public LedgerBookBuilder TestData1()
        {
            // Test Data 1
            Name = "Test Data 1 Book";
            StorageKey = @"C:\Folder\book1.xml";
            Modified = new DateTime(2013, 12, 16);
            WithLedger(new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts"))
                .WithLedger(new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Phone and Internet"))
                .WithLedger(new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power bills"))
                .WithReconciliation(
                    new DateTime(2013, 6, 15),
                    new BankBalance(StatementModelTestData.ChequeAccount, 2500))
                .WithLedgerEntries(
                    entryBuilder =>
                    {
                        entryBuilder
                            .ForLedger(TestDataConstants.HairBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(55M)
                                        .WithCredit(-45M, "Hair cut"));
                        entryBuilder
                            .ForLedger(TestDataConstants.PhoneBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(95M)
                                        .WithCredit(-86.43M, "Pay phones"));
                        entryBuilder
                            .ForLedger(TestDataConstants.PowerBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(140M)
                                        .WithCredit(-123.56M, "Power bill"));
                    })
                .WithReconciliation(
                    new DateTime(2013, 7, 15),
                    new BankBalance(StatementModelTestData.ChequeAccount, 3700))
                .WithLedgerEntries(
                    entryBuilder =>
                    {
                        entryBuilder
                            .ForLedger(TestDataConstants.HairBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(55M));
                        entryBuilder
                            .ForLedger(TestDataConstants.PhoneBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(95M)
                                        .WithCredit(-66.43M, "Pay phones"));
                        entryBuilder
                            .ForLedger(TestDataConstants.PowerBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(140M)
                                        .WithCredit(-145.56M, "Power bill"));
                    })
                .WithReconciliation(
                    new DateTime(2013, 8, 15),
                    new BankBalance(StatementModelTestData.ChequeAccount, 2950))
                .WithLedgerEntries(
                    entryBuilder =>
                    {
                        entryBuilder
                            .ForLedger(TestDataConstants.HairBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(55M));
                        entryBuilder
                            .ForLedger(TestDataConstants.PhoneBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(95M)
                                        .WithCredit(-67.43M, "Pay phones"));
                        entryBuilder
                            .ForLedger(TestDataConstants.PowerBucketCode)
                            .WithTransactions(
                                txnBuilder =>
                                    txnBuilder.WithBudgetCredit(140M)
                                        .WithCredit(-98.56M, "Power bill"));
                    });

            return this;
        }

        public LedgerBookBuilder WithLedger(ExpenseBucket bucket, decimal openingBalance = 0, Engine.BankAccount.Account account = null)
        {
            if (this.ledgerBuckets.Any(b => b.BudgetBucket.Code == bucket.Code))
            {
                throw new DuplicateNameException("Ledger Bucket already exists in collection.");
            }

            if (account == null)
            {
                account = StatementModelTestData.ChequeAccount;
            }

            var ledger = new LedgerBucket { BudgetBucket = bucket, StoredInAccount = account };
            this.ledgerBuckets.Add(ledger);
            this.openingBalances.Add(ledger, openingBalance);

            return this;
        }

        public ReconciliationTestDataBuilder WithReconciliation(DateTime reconDate, params BankBalance[] bankBalances)
        {
            this.tempBankBalances = bankBalances;
            this.tempReconDate = reconDate;

            var reconBuilder = new ReconciliationTestDataBuilder(this);
            return reconBuilder;
        }

        public LedgerBookBuilder WithUnlockFlagSet()
        {
            this.lockWhenFinished = false;
            return this;
        }

        private void SetReconciliation(IReadOnlyDictionary<LedgerBucket, IEnumerable<LedgerTransaction>> ledgerTransactions, string remarks)
        {
            var recon = new LedgerEntryLine(this.tempReconDate, this.tempBankBalances) { Remarks = remarks };
            LedgerEntryLine previousRecon = Reconciliations.OrderByDescending(r => r.Date).FirstOrDefault();
            var entries = new List<LedgerEntry>();

            foreach (LedgerBucket ledgerBucket in this.ledgerBuckets)
            {
                decimal openingBalance;
                if (previousRecon == null)
                {
                    openingBalance = this.openingBalances[ledgerBucket];
                }
                else
                {
                    LedgerEntry previousEntry = previousRecon.Entries.Single(e => e.LedgerBucket == ledgerBucket);
                    openingBalance = previousEntry.Balance;
                }
                var entry = new LedgerEntry
                {
                    LedgerBucket = ledgerBucket,
                    Balance = openingBalance
                };
                entry.SetTransactionsForTesting(ledgerTransactions[ledgerBucket].ToList());
                entries.Add(entry);
            }

            recon.SetEntriesForTesting(entries);
            this.reconciliations.Add(recon);
        }

        public class LedgerEntryTestDataBuilder
        {
            private readonly Dictionary<LedgerBucket, IEnumerable<LedgerTransaction>> ledgerTransactions = new Dictionary<LedgerBucket, IEnumerable<LedgerTransaction>>();
            private string nextLedgerCode;

            public LedgerEntryTestDataBuilder(IEnumerable<LedgerBucket> ledgers)
            {
                foreach (LedgerBucket ledgerBucket in ledgers)
                {
                    this.ledgerTransactions.Add(ledgerBucket, new List<LedgerTransaction>());
                }
            }

            public IReadOnlyDictionary<LedgerBucket, IEnumerable<LedgerTransaction>> LedgerTransactions => this.ledgerTransactions;

            public LedgerEntryTestDataBuilder ForLedger(string ledgerCode)
            {
                this.nextLedgerCode = ledgerCode;
                return this;
            }

            public LedgerEntryTestDataBuilder WithTransactions(Action<TransactionTestDataBuilder> transactionsCreator)
            {
                var txnBuilder = new TransactionTestDataBuilder();
                transactionsCreator(txnBuilder);
                LedgerBucket ledger = this.ledgerTransactions.Keys.Single(l => l.BudgetBucket.Code == this.nextLedgerCode);
                this.ledgerTransactions[ledger] = txnBuilder.Transactions;
                return this;
            }
        }

        public class ReconciliationTestDataBuilder
        {
            private readonly LedgerBookBuilder bookBuilder;
            private string remarks;

            public ReconciliationTestDataBuilder(LedgerBookBuilder bookBuilder)
            {
                this.bookBuilder = bookBuilder;
            }

            public LedgerBookBuilder WithLedgerEntries(Action<LedgerEntryTestDataBuilder> createEntries)
            {
                var entryBuilder = new LedgerEntryTestDataBuilder(this.bookBuilder.LedgerBuckets);
                createEntries(entryBuilder);
                this.bookBuilder.SetReconciliation(entryBuilder.LedgerTransactions, this.remarks);
                return this.bookBuilder;
            }

            public ReconciliationTestDataBuilder WithRemarks(string reconRemarks)
            {
                this.remarks = reconRemarks;
                return this;
            }
        }

        public class TransactionTestDataBuilder
        {
            private readonly List<LedgerTransaction> transactions = new List<LedgerTransaction>();

            public IEnumerable<LedgerTransaction> Transactions => this.transactions;

            public TransactionTestDataBuilder WithBudgetCredit(decimal amount, DateTime? date = null, string automatchingRef = null)
            {
                BudgetCreditLedgerTransaction budgetTxn = this.transactions.OfType<BudgetCreditLedgerTransaction>().FirstOrDefault();
                if (budgetTxn == null)
                {
                    budgetTxn = new BudgetCreditLedgerTransaction { AutoMatchingReference = automatchingRef, Date = date, Narrative = "Budgeted Amount" };
                    this.transactions.Add(budgetTxn);
                }

                budgetTxn.Amount = amount;
                return this;
            }

            public TransactionTestDataBuilder WithCredit(decimal amount, string narrative, DateTime? date = null, string automatchingRef = null)
            {
                this.transactions.Add(new CreditLedgerTransaction { Amount = amount, Narrative = narrative, AutoMatchingReference = automatchingRef, Date = date });
                return this;
            }
        }
    }
}