using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using NSubstitute;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit.Mobile;

public class MobileDataExporterTests
{
    private readonly MobileDataExporter exporter;
    private readonly IFileReaderWriter mockReaderWriter;
    private readonly ITestOutputHelper outputHelper;
    private readonly GlobalFilterCriteria testCriteria = new() { BeginDate = new DateOnly(2013, 7, 15), EndDate = new DateOnly(2013, 8, 14) };
    private readonly LedgerBook testLedger = LedgerBookTestData.TestData1();
    private readonly StatementModel testStatement = StatementModelTestData.TestData1();

    public MobileDataExporterTests(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
        var mockReaderWriterSelector = Substitute.For<IReaderWriterSelector>();
        var mockEnvironmentFolders = Substitute.For<IEnvironmentFolders>();
        this.mockReaderWriter = Substitute.For<IFileReaderWriter>();
        mockReaderWriterSelector.SelectReaderWriter(false).Returns(this.mockReaderWriter);
        mockEnvironmentFolders.ApplicationDataFolder().Returns(Task.FromResult("D:\\temp"));
        this.exporter = new MobileDataExporter(new LedgerCalculation(), mockReaderWriterSelector, mockEnvironmentFolders);
    }

    [Fact]
    public void CreateExportObject_ShouldReturn_WhenGivenTestData1()
    {
        var expected = new SummarisedLedgerMobileData
        {
            Exported = new DateTime(2025, 1, 1), Title = "Test Data 3 Budget", LastTransactionImport = new DateTime(2013, 08, 15), StartOfMonth = new DateOnly(2013, 7, 15)
        };
        expected.LedgerBuckets.AddRange([
            new SummarisedLedgerBucket
            {
                AccountName = TestDataConstants.ChequeAccountName,
                BucketCode = " " + SurplusBucket.SurplusCode,
                BucketType = "Surplus",
                Description = SurplusBucket.SurplusDescription,
                MonthlyBudgetAmount = 1830M,
                OpeningBalance = 3586.98M,
                RemainingBalance = 3481.66M
            },
            new SummarisedLedgerBucket
            {
                AccountName = TestDataConstants.ChequeAccountName,
                BucketCode = TestDataConstants.HairBucketCode,
                BucketType = "Accumulated Expense",
                Description = "Hair cuts wheelbarrow.",
                MonthlyBudgetAmount = 65M,
                OpeningBalance = 65M,
                RemainingBalance = 65M
            },
            new SummarisedLedgerBucket
            {
                AccountName = TestDataConstants.ChequeAccountName,
                BucketCode = TestDataConstants.PhoneBucketCode,
                BucketType = "Spent Monthly/Fortnightly Expense",
                Description = "Poo bar",
                MonthlyBudgetAmount = 120M,
                OpeningBalance = 37.14M,
                RemainingBalance = -21.05M
            },
            new SummarisedLedgerBucket
            {
                AccountName = TestDataConstants.ChequeAccountName,
                BucketCode = TestDataConstants.PowerBucketCode,
                BucketType = "Spent Monthly/Fortnightly Expense",
                Description = "Power ",
                MonthlyBudgetAmount = 185M,
                OpeningBalance = 10.88M,
                RemainingBalance = -84.27M
            }
        ]);

        var result = this.exporter.CreateExportObject(this.testStatement, BudgetModelTestData.CreateTestData5(), this.testLedger, this.testCriteria);
        expected.Exported = result.Exported; //Ignoring DateTime checking here

        result.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public void CreateExportObject_ShouldThrowArgumentException_WhenFilterDatesAreNull()
    {
        var filter = new GlobalFilterCriteria();
        Assert.Throws<ArgumentException>(() => this.exporter.CreateExportObject(this.testStatement, new BudgetModel(), this.testLedger, filter));
    }

    [Fact]
    public void CreateExportObject_ShouldThrowArgumentNullException_WhenCurrentBudgetIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => this.exporter.CreateExportObject(this.testStatement, null!, this.testLedger, this.testCriteria));
    }

    [Fact]
    public void CreateExportObject_ShouldThrowArgumentNullException_WhenFilterIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => this.exporter.CreateExportObject(this.testStatement, new BudgetModel(), this.testLedger, null!));
    }

    [Fact]
    public void CreateExportObject_ShouldThrowArgumentNullException_WhenLedgerBookIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => this.exporter.CreateExportObject(this.testStatement, new BudgetModel(), null!, this.testCriteria));
    }

    [Fact]
    public void CreateExportObject_ShouldThrowArgumentNullException_WhenTransactionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => this.exporter.CreateExportObject(null!, new BudgetModel(), LedgerBookTestData.TestData1(), this.testCriteria));
    }

    [Fact]
    public async Task SaveCopyAsync_ShouldCallWriteToDiskAsync()
    {
        var dataObject = new SummarisedLedgerMobileData { Exported = DateTime.Now, LastTransactionImport = DateTime.Now, Title = BudgetModelTestData.CreateTestData5().Name };

        await this.exporter.SaveCopyAsync(dataObject);

        await this.mockReaderWriter.Received(1).WriteToDiskAsync(Arg.Is("D:\\temp\\MobileDataExport.json"), Arg.Is<string>(s => s.Length > 50));
    }

    [Fact]
    public void Serialise_ShouldReturnJsonString()
    {
        var dataObject = new SummarisedLedgerMobileData { Exported = DateTime.Now, LastTransactionImport = DateTime.Now, StartOfMonth = DateOnlyExt.Today(), Title = "Test" };

        var json = this.exporter.Serialise(dataObject);

        json.ShouldNotBeEmpty();
        json.ShouldContain("\"Title\":\"Test\"");
    }
}
