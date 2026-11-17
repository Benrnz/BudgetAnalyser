using BudgetAnalyser.Engine.Transactions;

namespace BudgetAnalyser.Engine.BankFeed;

/// <summary>
///     Downloads bank feed transactions for a set of mapped accounts and converts them into Budget Analyser <see cref="Transaction" />s.
///     Every account is downloaded in full before anything is returned. If any account fails an exception is thrown and nothing is returned, so a partial import can never happen.
/// </summary>
[AutoRegisterWithIoC]
internal class BankFeedTransactionsImporter
{
    private readonly IBankFeedConfigurationService configurationService;
    private readonly ILogger logger;
    private readonly MapperBankFeedTransactionToTransaction mapper;
    private readonly IBankFeedProvider provider;

    public BankFeedTransactionsImporter(IBankFeedProvider provider, IBankFeedConfigurationService configurationService, ITransactionTypeRepository transactionTypeRepository, ILogger logger)
    {
        this.provider = provider ?? throw new ArgumentNullException(nameof(provider));
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.mapper = new MapperBankFeedTransactionToTransaction(transactionTypeRepository ?? throw new ArgumentNullException(nameof(transactionTypeRepository)));
    }

    /// <summary>
    ///     Downloads and converts the transactions for each of the given account mappings.
    /// </summary>
    /// <param name="range">The inclusive date range to download. <see cref="ImportDateRange.Start" /> must not be null.</param>
    /// <param name="mappings">The account mappings to download.</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    /// <returns>One entry per mapping, in the same order.</returns>
    /// <exception cref="BankFeedAuthenticationException">Thrown when the credentials are rejected. The connection is marked as no longer verified.</exception>
    /// <exception cref="BankFeedException">Thrown when any account cannot be downloaded.</exception>
    public async Task<IReadOnlyList<BankFeedAccountDownload>> DownloadAsync(ImportDateRange range, IReadOnlyList<BankFeedAccountMapping> mappings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mappings);
        if (range.Start is null)
        {
            throw new ArgumentException("A start date is required to download transactions.", nameof(range));
        }

        var downloads = new List<BankFeedAccountDownload>();
        try
        {
            foreach (var mapping in mappings)
            {
                var feedTransactions = await this.provider.GetTransactionsAsync(mapping.ExternalAccountId, range, cancellationToken);
                var transactions = feedTransactions.Select(t => this.mapper.ToModel(t, mapping, this.provider.ProviderKey)).ToList();
                this.logger.LogInfo(_ => $"Downloaded {transactions.Count} bank feed transactions for account {mapping.Account.Name}.");
                downloads.Add(new BankFeedAccountDownload(mapping, transactions));
            }
        }
        catch (BankFeedAuthenticationException)
        {
            this.configurationService.NotifyCredentialsRejected();
            throw;
        }

        return downloads;
    }
}

/// <summary>
///     The converted transactions downloaded for one account mapping.
/// </summary>
internal record BankFeedAccountDownload(BankFeedAccountMapping Mapping, IReadOnlyList<Transaction> Transactions);
