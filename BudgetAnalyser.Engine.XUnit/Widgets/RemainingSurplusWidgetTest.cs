using System.CodeDom.Compiler;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.XUnit.Helpers;
using BudgetAnalyser.Engine.XUnit.TestData;
using BudgetAnalyser.Engine.XUnit.TestHarness;
using Shouldly;
using Xunit.Abstractions;

#pragma warning disable CS8601 // Possible null reference assignment. // GENERATED CODE

namespace BudgetAnalyser.Engine.XUnit.Widgets;

public class RemainingSurplusWidgetTest : IDisposable
{
    private readonly BucketBucketRepoAlwaysFind bucketRepo = new();
    private readonly IBudgetCurrencyContext budgetTestData;
    private readonly GlobalFilterCriteria criteriaTestData = new() { BeginDate = new DateTime(2015, 10, 20), EndDate = new DateTime(2015, 11, 19) };
    private readonly LedgerBook ledgerBookTestData;
    private readonly LedgerCalculation ledgerCalculation;
    private readonly XUnitOutputWriter outputWriter;
    private readonly StatementModel statementTestData;
    private readonly RemainingSurplusWidget subject = new();

    public RemainingSurplusWidgetTest(ITestOutputHelper outputHelper)
    {
        this.outputWriter = new XUnitOutputWriter(outputHelper);
        StatementModelTestDataForThisTest.AccountTypeRepo = new InMemoryAccountTypeRepository();
        StatementModelTestDataForThisTest.BudgetBucketRepo = this.bucketRepo;
        this.statementTestData = StatementModelTestDataForThisTest.TestDataGenerated();

        var budgetModel = BudgetModelTestData.CreateTestData1();
        this.budgetTestData = new BudgetCurrencyContext(new BudgetCollection(budgetModel), budgetModel);

        this.ledgerBookTestData = new LedgerBookBuilder { StorageKey = "RemainingSurplusWidgetTest.xml", Modified = new DateTime(2015, 11, 23), Name = "Smith Budget 2015" }
            .IncludeLedger(LedgerBookTestData.PhoneLedger, 130M)
            .IncludeLedger(LedgerBookTestData.CarMtcLedger, 90M)
            .IncludeLedger(LedgerBookTestData.PowerLedger)
            .AppendReconciliation(
                new DateTime(2015, 10, 20),
                new BankBalance(LedgerBookTestData.ChequeAccount, 4502.75M))
            .WithReconciliationEntries(
                entryBuilder =>
                {
                    entryBuilder.WithLedger(LedgerBookTestData.PhoneLedger).HasNoTransactions();
                    entryBuilder.WithLedger(LedgerBookTestData.CarMtcLedger).HasNoTransactions();
                    entryBuilder.WithLedger(LedgerBookTestData.PowerLedger)
                        .AppendTransactions(txnBuilder => { txnBuilder.WithCredit(3000M, "Oct Savings", new DateTime(2015, 10, 20), "automatchref12"); });
                })
            .Build();

        this.ledgerCalculation = new LedgerCalculation(new FakeLogger());
    }

    public void Dispose()
    {
        this.outputWriter.Dispose();
        this.statementTestData.Dispose();
    }

    [Fact]
    public void OutputTestData()
    {
        this.ledgerBookTestData.Output(true, this.outputWriter);
        this.budgetTestData.Output(this.outputWriter);
        this.statementTestData.Output(DateTime.MinValue, this.outputWriter);
    }

    [Fact]
    public void Update_ShouldBeZero_WhenSurplusIsOverdrawnAndExcludeAutoMatchedTransactionsInCalculation()
    {
        this.budgetTestData.Model.Incomes.First().Amount = 2500;
        this.subject.Update(this.budgetTestData, this.statementTestData, this.criteriaTestData, this.bucketRepo, this.ledgerBookTestData, this.ledgerCalculation, new FakeLogger());
        // Begin Date 20/10/2015 EndDate 19/11/2015
        // Starting Surplus is: 2175.00
        // Total Surplus transactions are: -835.69
        // Plus any overspent ledgers (that must also come out of surplus funds): -873.38
        // Resulting Balance = 2175 - 835.69 - 873.38 = 465.93
        this.subject.Value.ShouldBe(465.93);
    }

    [Fact]
    public void Update_ShouldCalculate_ExcludeAutoMatchedTransactionsInCalculation()
    {
        this.subject.Update(this.budgetTestData, this.statementTestData, this.criteriaTestData, this.bucketRepo, this.ledgerBookTestData, this.ledgerCalculation);
        // Begin Date 20/10/2015 EndDate 19/11/2015
        // Starting Surplus is: 1175.00
        // Total Surplus transactions are: -835.69
        // Plus any overspent ledgers (that must also come out of surplus funds): -873.38
        // Resulting Balance = 1175 - 835.69 - 873.38 = -534.07 (Grim)
        this.subject.Value.ShouldBe(0);
    }

    [Fact]
    public void Update_ShouldCalculateRemainingSurplus()
    {
        // Remove this transaction from the test data set
        // 23-Oct-15  Lorem Ipsum Dol SURPLUS    automatchref12                   -3,000.00 CHEQUE          c66eb722-6d03-48b2-b985-6721701a01ae
        // Also remove all other accounts, filtering to CHEQUE only
        // This will create a more realistic data set.
        var removeId = Guid.Parse("c66eb722-6d03-48b2-b985-6721701a01ae");
        var myTransactions = this.statementTestData.AllTransactions
            .ToList()
            .Where(t => t.Account == StatementModelTestData.ChequeAccount)
            .Where(t => t.Id != removeId)
            .ToList();
        var myStatement = this.statementTestData.LoadTransactions(myTransactions);

        this.ledgerBookTestData.Output(true, this.outputWriter);
        this.budgetTestData.Output(this.outputWriter);
        myStatement.Output(DateTime.MinValue, this.outputWriter);

        this.subject.Update(this.budgetTestData, myStatement, this.criteriaTestData, this.bucketRepo, this.ledgerBookTestData, this.ledgerCalculation, new FakeLogger());

        this.subject.Value.ShouldBe(607.73);
        /*
        19-Nov-15
        Budget Surplus: $1175 - 30 - 2 - 40 - 8.50 - 27.74        = 1066.76
        Car MTC $90 - 130                                         =  -40.00 (OVERDRAWN LEDGER)
        PHNET  $130 - 55 - 24.10 - 444.63 - 25.30                 = -419.03 (OVERDRAWN LEDGER)
        POWER $3000 - 17.32                                       = 2932.68
        --------------------------------------------------------------------
                                                                     607.73
         */
    }

    private static class StatementModelTestDataForThisTest
    {
        public static IAccountTypeRepository AccountTypeRepo { get; set; } = null!;
        public static IBudgetBucketRepository BudgetBucketRepo { get; set; } = null!;

        /// <summary>THIS IS GENERATED CODE </summary>
        [GeneratedCode("StatementModelTestDataGenerator.GenerateCSharp", "11/23/2015 13:04:40")]
        public static StatementModel TestDataGenerated()
        {
            var model = new StatementModel(new FakeLogger()) { StorageKey = @"C:\Foo\StatementModel.csv", LastImport = new DateTime(2015, 11, 21) };

            var transactions = new List<Transaction>
            {
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -55.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 10, 21),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ce628396-3f6b-4980-88ff-e4ea68a5c804"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -24.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 10, 21),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("67f9cbff-18ca-4d11-a5ef-22988e04584a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -3000.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("c66eb722-6d03-48b2-b985-6721701a01ae"),
                    Reference1 = "automatchref12",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -130.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("233a8a87-5583-47ef-ae72-592aa9c10eb8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 3000.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("9ecd767b-b3c7-4c15-a40f-1c0098e422ed"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 130.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("1a0dd7ef-2962-4c8c-85cb-7d3e64748211"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -27.44M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FUEL"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7ac24965-e21e-4f63-b3e4-9ec391843f56"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -73.34M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7a9dd17a-1957-4ac4-8175-28a93d9a1eb9"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -32.70M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 1),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("31b7af5c-4429-4cf7-b0b4-54ff8d72eb1d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -49.70M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 1),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("246fe376-d6b2-4a09-a881-f07d1577ffa7"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -30.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 2),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("886b4bd5-455c-42bb-826c-cc61edcec84f"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Atm Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -120.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 2),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8673ce8f-629a-413f-b7c9-afbcc123cf80"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Atm Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -10.79M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 3),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2a3b17cf-c4a0-4c01-893b-73677cc650ad"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.30M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 5),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2a5b1821-802d-4b5e-af72-728dde935931"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -7.99M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 5),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("b87d9668-6d17-444f-89ef-d53cd1ba826c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -17.32M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("POWER"),
                    Date = new DateTime(2015, 11, 6),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("af952e91-9ca8-442e-9569-13b8796c9eef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -42.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 6),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("00172bd9-0ec4-4d92-94bf-d533299e0dc5"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -30.60M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 6),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("63a9d1dd-a28e-4561-8443-570af6f1da6a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -11.47M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e53f7c07-f06b-4a99-8310-57d6f7aef2e8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -31.49M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("810c31e3-bdfb-4aef-a6dd-b96a3789e27f"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -14.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("34249540-ac64-4392-9fa7-c0e10ecc06af"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -2.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 9),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f705a9c8-533c-451e-9e4b-70266679af58"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -38.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 11),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("b56e8c82-501e-4c77-b2ba-0fa9e86e19ef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 12),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("eb465d02-e82c-4d5d-884a-2adb1c86cf4c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -9.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 12),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("0ec1c6b3-8f4c-47d5-99e2-c01168cbae2c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -40.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ac3a08f2-3274-4667-b382-9f23e81fa9e8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -444.63M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ce618893-06ab-4dab-8fd8-5578e6b90700"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -11.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("5db38e40-a58b-47bb-9b1c-077b5f8cecc5"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -159.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("92a6d37b-a846-4b2c-b9b3-14ba85c70295"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -24.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f32b2ed3-717f-41b5-8892-d48df86579ef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -471.01M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8626f06a-5ba6-45e1-b197-4c0b0ab927c3"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.64M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("0f3d74ba-4456-41a1-a58e-b006534b664b"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -141.79M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 15),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("15bde657-e077-42a3-b0eb-1b49f76ba17d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -5.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 16),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("860cce1e-abca-4520-8a03-6e73a2ddca13"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -8.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 18),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8a6cd650-77cc-427b-a481-ac4b286ccfd4"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -27.74M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("840acbce-af64-4ce8-a3e9-d33beea48bdb"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Payment")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -25.30M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("9e18b22f-9623-44e7-b391-e868b482d545"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -45.98M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("212a5450-81c7-4441-a851-84a1830e7a4a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -3.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("aeb3c1ac-fb5a-4164-8125-3b5cf3487616"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new()
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -31.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("02861f41-a92c-4ffd-bd22-8194dc402f8d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                }
            };

            return model.LoadTransactions(transactions);
        }
    }
}
