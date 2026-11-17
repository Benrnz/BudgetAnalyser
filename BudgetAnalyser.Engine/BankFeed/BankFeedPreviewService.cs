using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed;

[AutoRegisterWithIoC]
internal class BankFeedPreviewService(BankFeedTransactionsImporter importer) : IBankFeedPreviewService
{
    private readonly BankFeedTransactionsImporter importer = importer ?? throw new ArgumentNullException(nameof(importer));

    public async Task<IReadOnlyList<Transaction>> PreviewAsync(BankFeedAccountMapping mapping, DateOnly today, int count, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var end = TransactionsCalculations.PreviousBusinessDay(today);
        var range = new ImportDateRange(end.AddDays(1 - IBankFeedPreviewService.PreviewDays), end);
        var downloads = await this.importer.DownloadAsync(range, [mapping], cancellationToken);
        return downloads.Single().Transactions
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToList();
    }
}
