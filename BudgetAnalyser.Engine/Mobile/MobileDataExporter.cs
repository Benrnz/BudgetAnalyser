using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    ///     A class to extract and summarise data to upload to the web to serve to the mobile app.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class MobileDataExporter : IMobileDataExporter
    {
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
            [NotNull] StatementModel transactions,
            [NotNull] BudgetModel currentBudget,
            [NotNull] LedgerBook ledgerBook,
            [NotNull] GlobalFilterCriteria filter)
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
                return null;
            }

            var export = new SummarisedLedgerMobileData
            {
                Exported = DateTime.Now,
                LastTransactionImport = transactions.LastImport,
                Title = currentBudget.Name,
                StartOfMonth = filter.BeginDate.Value
            };

            var latestRecon = this.calculator.LocateApplicableLedgerLine(ledgerBook, filter);
            if (latestRecon is null)
            {
                return null;
            }

            var ledgerList = new List<SummarisedLedgerBucket>();
            var ledgerLine = this.calculator.LocateApplicableLedgerLine(ledgerBook, filter.BeginDate.Value, filter.EndDate.Value);
            IDictionary<BudgetBucket, decimal> currentBalances = this.calculator.CalculateCurrentPeriodLedgerBalances(ledgerLine, filter, transactions);
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
                    MonthlyBudgetAmount = currentBudget.Expenses.First(e => e.Bucket.Code == entry.LedgerBucket.BudgetBucket.Code).Amount
                });
            }

            var accounts = string.Empty;
            foreach (var account in latestRecon.BankBalances)
            {
                accounts = $"{accounts}, {account.Account.Name}";
            }
            ledgerList.Add(new SummarisedLedgerBucket
            {
                AccountName = accounts,
                MonthlyBudgetAmount = -1, // Do not show, not relevant
                RemainingBalance = currentBalances[new SurplusBucket()],
                OpeningBalance = latestRecon.CalculatedSurplus,
                BucketCode = " " + SurplusBucket.SurplusCode,
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
            return JsonConvert.SerializeObject(dataExport);
        }

        private async Task<string> GetFileName()
        {
            return Path.Combine(await this.environmentFolders.LogFolder(), "MobileDataExport.json");
        }
    }
}