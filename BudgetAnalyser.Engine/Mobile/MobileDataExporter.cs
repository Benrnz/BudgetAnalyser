using System.Text.Json;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Mobile;

/// <summary>
///     A class to extract and summarise data to upload to the web to serve to the mobile app.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class MobileDataExporter : IMobileDataExporter
{
    internal const string MobileDataExportName = "MobileDataExport.json";
    private readonly LedgerCalculation calculator;
    private readonly IEnvironmentFolders environmentFolders;
    private readonly IReaderWriterSelector readerWriterSelector;

    /// <summary>
    ///     Initialises an instance of the <see cref="MobileDataExporter" /> class.
    /// </summary>
    public MobileDataExporter(LedgerCalculation calculator, IReaderWriterSelector readerWriterSelector, IEnvironmentFolders environmentFolders)
    {
        this.calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        this.readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
        this.environmentFolders = environmentFolders ?? throw new ArgumentNullException(nameof(environmentFolders));
    }

    /// <summary>
    ///     Create the export object
    /// </summary>
    /// <returns>An object containing the summarised data.</returns>
    public SummarisedLedgerMobileData CreateExportObject(
        StatementModel transactions,
        BudgetModel currentBudget,
        LedgerBook ledgerBook,
        GlobalFilterCriteria filter)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        if (currentBudget is null)
        {
            throw new ArgumentNullException(nameof(currentBudget));
        }

        if (ledgerBook is null)
        {
            throw new ArgumentNullException(nameof(ledgerBook));
        }

        if (filter is null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        if (filter.BeginDate is null || filter.EndDate is null)
        {
            throw new ArgumentException("Date filters are null and not set to a date range.");
        }

        var export = new SummarisedLedgerMobileData { Exported = DateTime.Now, LastTransactionImport = transactions.LastImport, Title = currentBudget.Name, StartOfMonth = filter.BeginDate.Value };

        var latestRecon = this.calculator.LocateApplicableLedgerLine(ledgerBook, filter);
        if (latestRecon is null)
        {
            return export;
        }

        var ledgerList = new List<SummarisedLedgerBucket>();
        var currentBalances = this.calculator.CalculateCurrentPeriodLedgerBalances(latestRecon, filter, transactions);
        foreach (var entry in latestRecon.Entries)
        {
            ledgerList.Add(new SummarisedLedgerBucket
            {
                AccountName = entry.LedgerBucket.StoredInAccount.Name,
                RemainingBalance = currentBalances[entry.LedgerBucket.BudgetBucket],
                OpeningBalance = entry.Balance,
                BucketCode = entry.LedgerBucket.BudgetBucket.Code,
                BucketType = entry.LedgerBucket.BudgetBucket.TypeDescription,
                Description = entry.LedgerBucket.BudgetBucket.Description,
                MonthlyBudgetAmount = currentBudget.Expenses.First(e => e.BucketCode == entry.LedgerBucket.BudgetBucket.Code).Amount
            });
        }

        var accounts = latestRecon.BankBalances.First().Account.Name;
        foreach (var account in latestRecon.BankBalances.Skip(1))
        {
            accounts = $"{accounts}, {account.Account.Name}";
        }

        ledgerList.Add(new SummarisedLedgerBucket
        {
            AccountName = accounts,
            MonthlyBudgetAmount = currentBudget.Surplus,
            RemainingBalance = currentBalances[new SurplusBucket()],
            OpeningBalance = latestRecon.CalculatedSurplus,
            BucketCode = " " + SurplusBucket.SurplusCode, // For sorting purposes
            BucketType = "Surplus",
            Description = SurplusBucket.SurplusDescription
        });

        export.LedgerBuckets.AddRange(ledgerList.OrderBy(l => l.BucketCode));
        return export;
    }

    /// <summary>
    ///     Save a copy of the data export file locally.
    /// </summary>
    public async Task SaveCopyAsync(SummarisedLedgerMobileData dataObject)
    {
        var serialised = Serialise(dataObject);
        var writer = this.readerWriterSelector.SelectReaderWriter(false);
        await writer.WriteToDiskAsync(await GetFileName(), serialised);
    }

    public string Serialise(SummarisedLedgerMobileData dataExport)
    {
        return JsonSerializer.Serialize(dataExport, new JsonSerializerOptions());
    }

    private async Task<string> GetFileName()
    {
        return Path.Combine(await this.environmentFolders.ApplicationDataFolder(), MobileDataExportName);
    }
}
