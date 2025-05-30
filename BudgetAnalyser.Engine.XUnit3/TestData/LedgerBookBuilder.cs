﻿using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.XUnit.TestHarness;

namespace BudgetAnalyser.Engine.XUnit.TestData;

/// <summary>
///     Trialing a Fluent Builder pattern instead of a Object Mother pattern
/// </summary>
public class LedgerBookBuilder
{
    private readonly List<LedgerBucket> ledgerBuckets = new();
    private readonly Dictionary<LedgerBucket, decimal> openingBalances = new();
    private readonly List<LedgerEntryLine> reconciliations = new();
    private bool lockWhenFinished = true;
    private IEnumerable<BankBalance> tempBankBalances = Array.Empty<BankBalance>();
    private DateOnly tempReconDate;

    public IEnumerable<LedgerBucket> LedgerBuckets => this.ledgerBuckets;
    public DateTime Modified { get; set; } = new(2013, 12, 16);

    public string Name { get; set; } = "Test Data Book - built by LedgerBookBuilder";

    public IEnumerable<LedgerEntryLine> Reconciliations => this.reconciliations;
    public string StorageKey { get; set; } = "C:\\Folder\\book1.xml";

    public ReconciliationTestDataBuilder AppendReconciliation(DateOnly reconDate, params BankBalance[] bankBalances)
    {
        this.tempBankBalances = bankBalances;
        this.tempReconDate = reconDate;

        var reconBuilder = new ReconciliationTestDataBuilder(this);
        return reconBuilder;
    }

    public LedgerBook Build()
    {
        var book = new LedgerBook(this.reconciliations) { Name = Name, Modified = Modified, StorageKey = StorageKey };

        if (book.Reconciliations.None())
        {
            this.ledgerBuckets.ForEach(l => book.AddLedger(l));
        }

        LedgerBookTestData.Finalise(book, this.lockWhenFinished);
        return book;
    }

    public LedgerBookBuilder IncludeLedger(LedgerBucket ledger, decimal openingBalance = 0)
    {
        if (this.ledgerBuckets.Any(b => b.BudgetBucket.Code == ledger.BudgetBucket.Code))
        {
            throw new DuplicateNameException("Ledger Bucket already exists in collection.");
        }

        if (ledger.StoredInAccount is null)
        {
            ledger.StoredInAccount = StatementModelTestData.ChequeAccount;
        }

        this.ledgerBuckets.Add(ledger);
        this.openingBalances.Add(ledger, openingBalance);

        return this;
    }

    public LedgerBookBuilder TestData1()
    {
        // Test Data 1
        Name = "Test Data 1 Book";
        StorageKey = @"C:\Folder\book1.xml";
        Modified = new DateTime(2013, 12, 16);
        IncludeLedger(LedgerBookTestData.HairLedger)
            .IncludeLedger(LedgerBookTestData.PhoneLedger)
            .IncludeLedger(LedgerBookTestData.PowerLedger)
            .AppendReconciliation(
                new DateOnly(2013, 6, 15),
                new BankBalance(StatementModelTestData.ChequeAccount, 2500))
            .WithReconciliationEntries(
                entryBuilder =>
                {
                    entryBuilder
                        .WithLedger(LedgerBookTestData.HairLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(55M)
                                    .WithCredit(-45M, "Hair cut"));
                    entryBuilder
                        .WithLedger(LedgerBookTestData.PhoneLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(95M)
                                    .WithCredit(-86.43M, "Pay phones"));
                    entryBuilder
                        .WithLedger(LedgerBookTestData.PowerLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(140M)
                                    .WithCredit(-123.56M, "Power bill"));
                })
            .AppendReconciliation(
                new DateOnly(2013, 7, 15),
                new BankBalance(StatementModelTestData.ChequeAccount, 3700))
            .WithReconciliationEntries(
                entryBuilder =>
                {
                    entryBuilder
                        .WithLedger(LedgerBookTestData.HairLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(55M));
                    entryBuilder
                        .WithLedger(LedgerBookTestData.PhoneLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(95M)
                                    .WithCredit(-66.43M, "Pay phones"));
                    entryBuilder
                        .WithLedger(LedgerBookTestData.PowerLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(140M)
                                    .WithCredit(-145.56M, "Power bill"));
                })
            .AppendReconciliation(
                new DateOnly(2013, 8, 15),
                new BankBalance(StatementModelTestData.ChequeAccount, 2950))
            .WithReconciliationEntries(
                entryBuilder =>
                {
                    entryBuilder
                        .WithLedger(LedgerBookTestData.HairLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(55M));
                    entryBuilder
                        .WithLedger(LedgerBookTestData.PhoneLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(95M)
                                    .WithCredit(-67.43M, "Pay phones"));
                    entryBuilder
                        .WithLedger(LedgerBookTestData.PowerLedger)
                        .AppendTransactions(
                            txnBuilder =>
                                txnBuilder.WithBudgetCredit(140M)
                                    .WithCredit(-98.56M, "Power bill"));
                });

        return this;
    }

    public LedgerBookBuilder WithUnlockFlagSet()
    {
        this.lockWhenFinished = false;
        return this;
    }

    internal LedgerBookTestHarness BuildTestHarness(IReconciliationBuilder reconciliationBuilder)
    {
        var book = new LedgerBookTestHarness(this.reconciliations) { Name = Name, Modified = Modified, StorageKey = StorageKey };

        LedgerBookTestData.Finalise(book, this.lockWhenFinished);
        return book;
    }

    private void SetReconciliation(IReadOnlyDictionary<LedgerBucket, SpecificLedgerEntryTestDataBuilder> ledgers, string remarks)
    {
        var recon = new LedgerEntryLine(this.tempReconDate, this.tempBankBalances) { Remarks = remarks };
        var previousRecon = Reconciliations.OrderByDescending(r => r.Date).FirstOrDefault();
        var entries = new List<LedgerEntry>();

        foreach (var ledgerBucket in this.ledgerBuckets)
        {
            decimal openingBalance;
            if (previousRecon is null)
            {
                openingBalance = this.openingBalances[ledgerBucket];
            }
            else
            {
                var previousEntry = previousRecon.Entries.Single(e => e.LedgerBucket == ledgerBucket);
                openingBalance = previousEntry.Balance;
            }

            var entry = new LedgerEntry { LedgerBucket = ledgerBucket, Balance = openingBalance };
            entry.SetTransactionsForTesting(ledgers[ledgerBucket].Transactions.ToList());
            entries.Add(entry);
        }

        recon.SetEntriesForTesting(entries);
        this.reconciliations.Add(recon);
    }

    public class LedgerEntryTestDataBuilder
    {
        private readonly Dictionary<LedgerBucket, SpecificLedgerEntryTestDataBuilder> ledgers = new();

        public LedgerEntryTestDataBuilder(IEnumerable<LedgerBucket> ledgers)
        {
            foreach (var ledgerBucket in ledgers)
            {
                this.ledgers.Add(ledgerBucket, new SpecificLedgerEntryTestDataBuilder(this));
            }
        }

        public IReadOnlyDictionary<LedgerBucket, SpecificLedgerEntryTestDataBuilder> Ledgers => this.ledgers;

        public SpecificLedgerEntryTestDataBuilder WithLedger(LedgerBucket ledger)
        {
            return this.ledgers[ledger];
        }
    }

    public class ReconciliationTestDataBuilder
    {
        private readonly LedgerBookBuilder bookBuilder;
        private string remarks = string.Empty;

        public ReconciliationTestDataBuilder(LedgerBookBuilder bookBuilder)
        {
            this.bookBuilder = bookBuilder;
        }

        public LedgerBookBuilder WithNoEntries()
        {
            return this.bookBuilder;
        }

        public LedgerBookBuilder WithReconciliationEntries(Action<LedgerEntryTestDataBuilder> createEntries)
        {
            var entryBuilder = new LedgerEntryTestDataBuilder(this.bookBuilder.LedgerBuckets);
            createEntries(entryBuilder);
            this.bookBuilder.SetReconciliation(entryBuilder.Ledgers, this.remarks);
            return this.bookBuilder;
        }

        public ReconciliationTestDataBuilder WithRemarks(string reconRemarks)
        {
            this.remarks = reconRemarks;
            return this;
        }
    }

    public class SpecificLedgerEntryTestDataBuilder
    {
        private readonly LedgerEntryTestDataBuilder entryBuilder;
        private List<LedgerTransaction> ledgerTransactions = new();

        public SpecificLedgerEntryTestDataBuilder(LedgerEntryTestDataBuilder entryBuilder)
        {
            this.entryBuilder = entryBuilder;
        }

        public IEnumerable<LedgerTransaction> Transactions => this.ledgerTransactions;

        public SpecificLedgerEntryTestDataBuilder AppendTransactions(Action<TransactionTestDataBuilder> transactionsCreator)
        {
            //SpecificLedgerEntryTestDataBuilder
            var txnBuilder = new TransactionTestDataBuilder();
            transactionsCreator(txnBuilder);
            this.ledgerTransactions = txnBuilder.Transactions.ToList();
            return this;
        }

        public LedgerEntryTestDataBuilder HasNoTransactions()
        {
            this.ledgerTransactions = new List<LedgerTransaction>();
            return this.entryBuilder;
        }
    }

    public class TransactionTestDataBuilder
    {
        private readonly List<LedgerTransaction> transactions = new();

        public IEnumerable<LedgerTransaction> Transactions => this.transactions;

        public TransactionTestDataBuilder WithBudgetCredit(decimal amount, DateOnly? date = null, string? automatchingRef = null)
        {
            var budgetTxn = this.transactions.OfType<BudgetCreditLedgerTransaction>().FirstOrDefault();
            if (budgetTxn is null)
            {
                budgetTxn = new BudgetCreditLedgerTransaction { AutoMatchingReference = automatchingRef ?? string.Empty, Date = date, Narrative = "Budgeted Amount" };
                this.transactions.Add(budgetTxn);
            }

            budgetTxn.Amount = amount;
            return this;
        }

        public TransactionTestDataBuilder WithCredit(decimal amount, string narrative, DateOnly? date = null, string? automatchingRef = null)
        {
            this.transactions.Add(new CreditLedgerTransaction { Amount = amount, Narrative = narrative, AutoMatchingReference = automatchingRef ?? string.Empty, Date = date });
            return this;
        }
    }
}
