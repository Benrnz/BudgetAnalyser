using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Transactions;

/// <summary>
///     The "Download from bank feed" dialog. It suggests a date range from the existing transactions, lists the mapped accounts with how fresh the feed's data is, and
///     validates the choice. It downloads nothing itself: it returns the user's choice, or null if they cancel.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public partial class BankFeedImportController : ControllerBase, IShellDialogInteractivity
{
    /// <summary>
    ///     How many times the bank feed is checked for new data after a refresh is requested, before giving up waiting.
    /// </summary>
    internal const int MaxRefreshChecks = 20;

    private readonly IBankFeedConfigurationService configurationService;
    private readonly ILogger logger;
    private Guid dialogCorrelationId;
    private IReadOnlyList<Transaction> existingTransactions = [];
    private CancellationTokenSource? feedDetailsCancellation;
    private DateOnly? historyAvailableFrom;
    private TaskCompletionSource<BankFeedImportRequest?>? pendingRequest;
    private ImportDateRange suggestedRange;
    private int suggestedStartOverlapDays;

    public BankFeedImportController(IMessenger messenger, IBankFeedConfigurationService configurationService, ILogger logger) : base(messenger)
    {
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Messenger.Register<BankFeedImportController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDialogResponseReceived(m));
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, CanRefresh);
    }

    public ObservableCollection<BankFeedImportAccountRowViewModel> AccountRows { get; } = new();

    public bool CanExecuteCancelButton => true;

    [ObservableProperty]
    public partial bool CanExecuteOkButton { get; private set; }

    public bool CanExecuteSaveButton => false;

    [ObservableProperty]
    public partial DateOnly? FromDate { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; private set; }

    /// <summary>
    ///     True while the bank feed is refreshing its data from the banks at the user's request. Download is disabled until it finishes.
    /// </summary>
    [ObservableProperty]
    public partial bool IsRefreshing { get; private set; }

    /// <summary>
    ///     A problem that stops the download from starting at all, for example the credentials were rejected.
    /// </summary>
    [ObservableProperty]
    public partial string LoadError { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string OverlapWarning { get; private set; } = string.Empty;

    /// <summary>
    ///     Asks the bank feed to refresh its data from the banks, then waits for the accounts' "data as of" times to move forward.
    /// </summary>
    public IAsyncRelayCommand RefreshCommand { get; }

    /// <summary>
    ///     How long to wait between checks for new data after a refresh is requested.
    /// </summary>
    internal TimeSpan RefreshCheckInterval { get; set; } = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     The progress or outcome of the last refresh, or empty if none has been requested.
    /// </summary>
    [ObservableProperty]
    public partial string RefreshStatus { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string StaleWarning { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial string SuggestionText { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial DateOnly? ToDate { get; set; }

    /// <summary>
    ///     Why Download is disabled, or empty when it is enabled.
    /// </summary>
    [ObservableProperty]
    public partial string ValidationMessage { get; private set; } = string.Empty;

    /// <summary>
    ///     Shows the dialog and waits for the user. The account details are refreshed from the bank feed while the dialog is open.
    /// </summary>
    /// <returns>The user's choice, or null if they cancelled.</returns>
    public async Task<BankFeedImportRequest?> RequestUserInputAsync(TransactionsListModel transactions, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        Prepare(transactions, today);

        this.pendingRequest = new TaskCompletionSource<BankFeedImportRequest?>();
        this.dialogCorrelationId = Guid.NewGuid();
        Messenger.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Transactions, this, ShellDialogType.OkCancel)
        {
            CorrelationId = this.dialogCorrelationId,
            Title = "Download from bank feed"
        });

        var pending = this.pendingRequest.Task;

        // The bank feed check is cancelled as soon as the user responds, so pressing Cancel never waits for a slow or unreachable bank feed.
        using var loadCancellation = new CancellationTokenSource();
        this.feedDetailsCancellation = loadCancellation;
        try
        {
            await LoadFeedDetailsAsync(loadCancellation.Token);
            return await pending;
        }
        finally
        {
            this.feedDetailsCancellation = null;
        }
    }

    /// <summary>
    ///     Resets the dialog for the given transactions. Separate from <see cref="RequestUserInputAsync" /> so it can be tested without showing the dialog.
    /// </summary>
    internal void Prepare(TransactionsListModel transactions, DateOnly today)
    {
        this.existingTransactions = transactions.AllTransactions.ToList();
        this.historyAvailableFrom = null;

        // The usual suggestion starts the day after the latest transaction. Some bank feeds ask for the most recent day(s) to be downloaded again, to pick up late arrivals.
        this.suggestedStartOverlapDays = Math.Max(0, this.configurationService.ProviderSuggestedStartOverlapDays);
        var usualRange = TransactionsCalculations.CalculateSuggestedImportDateRange(this.existingTransactions, today);
        this.suggestedRange = usualRange with { Start = usualRange.Start?.AddDays(-this.suggestedStartOverlapDays) };
        SuggestionText = CreateSuggestionText(usualRange.Start?.AddDays(-1));
        LoadError = string.Empty;
        RefreshStatus = string.Empty;

        foreach (var row in AccountRows)
        {
            row.PropertyChanged -= OnRowPropertyChanged;
        }

        AccountRows.Clear();
        foreach (var mapping in this.configurationService.Mappings)
        {
            var row = new BankFeedImportAccountRowViewModel(mapping);
            row.PropertyChanged += OnRowPropertyChanged;
            AccountRows.Add(row);
        }

        // Set the dates last, they trigger validation.
        Today = today;
        FromDate = this.suggestedRange.Start;
        ToDate = this.suggestedRange.End;
        Revalidate();
    }

    internal DateOnly Today { get; private set; }

    /// <summary>
    ///     Applies the bank feed's current account details and history limit. Separate from <see cref="LoadFeedDetailsAsync" /> so it can be tested.
    /// </summary>
    internal void ApplyFeedDetails(IReadOnlyList<BankFeedAccount> feedAccounts, BankFeedConnectionStatus status)
    {
        this.historyAvailableFrom = status.HistoryAvailableFrom;
        ApplyFeedAccounts(feedAccounts);
    }

    internal void Revalidate()
    {
        UpdateWarnings();
        ValidationMessage = Validate();
        CanExecuteOkButton = !IsLoading && !IsRefreshing && LoadError.Length == 0 && ValidationMessage.Length == 0;
        RefreshCommand.NotifyCanExecuteChanged();
    }

    private void ApplyFeedAccounts(IReadOnlyList<BankFeedAccount> feedAccounts)
    {
        foreach (var row in AccountRows)
        {
            var feedAccount = feedAccounts.FirstOrDefault(a => a.ExternalId == row.Mapping.ExternalAccountId);
            row.FeedAccount = feedAccount;
            if (feedAccount is null)
            {
                row.IsAvailable = false;
                row.IsSelected = false;
                row.StatusText = "Not available: this account was not found in the bank feed.";
            }
            else if (!feedAccount.IsActive)
            {
                row.IsAvailable = false;
                row.IsSelected = false;
                row.StatusText = "Not available: this account is not currently connected in the bank feed.";
            }
        }

        Revalidate();
    }

    private string CreateSuggestionText(DateOnly? lastTransactionDate)
    {
        if (lastTransactionDate is null)
        {
            return "There are no transactions yet, so choose a From date. To is the last business day before today.";
        }

        var suggestion = string.Format(CultureInfo.CurrentCulture, "Suggested from your last transaction ({0:d}) to the last business day before today.", lastTransactionDate.Value);
        return this.suggestedStartOverlapDays switch
        {
            0 => suggestion,
            1 => suggestion + " That day is downloaded again in case the bank feed has added to it. Transactions you already have from the bank feed are skipped.",
            _ => suggestion + $" The last {this.suggestedStartOverlapDays} days are downloaded again in case the bank feed has added to them. Transactions you already have from the bank feed are skipped."
        };
    }

    private bool CanRefresh()
    {
        // There is nothing to refresh until the bank feed has answered with at least one of the mapped accounts.
        return !IsLoading && !IsRefreshing && AccountRows.Any(r => r.FeedAccount is not null);
    }

    /// <summary>
    ///     True if the account's data is newer than it was before the refresh was requested. An account that is no longer in the feed, or not connected, is not waited for.
    /// </summary>
    private static bool HasRefreshed(DateTimeOffset? dataAsOfBefore, BankFeedAccount? accountNow)
    {
        return accountNow is null
               || !accountNow.IsActive
               || (accountNow.DataAsOf is { } asOf && (dataAsOfBefore is null || asOf > dataAsOfBefore.Value));
    }

    private async Task LoadFeedDetailsAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        Revalidate();
        try
        {
            var accountsTask = this.configurationService.GetFeedAccountsAsync(cancellationToken);
            // Only the history limit is needed here. This does not record a connection test, so opening the dialog never rewrites the credentials file or disturbs the Settings tab.
            var statusTask = this.configurationService.GetConnectionStatusAsync(cancellationToken);

            // WaitAsync stops waiting immediately on cancellation, even if a request is slow to notice it has been cancelled.
            await Task.WhenAll(accountsTask, statusTask).WaitAsync(cancellationToken);
            ApplyFeedDetails(accountsTask.Result, statusTask.Result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The user closed the dialog before the bank feed answered. There is nothing to show.
        }
        catch (BankFeedAuthenticationException)
        {
            LoadError = "Your bank feed credentials were rejected. Update them on the Settings tab.";
        }
        catch (BankFeedException ex)
        {
            this.logger.LogWarning(_ => $"Unable to load bank feed account details: {ex.Message}");
            LoadError = $"Unable to reach the bank feed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            Revalidate();
        }
    }

    partial void OnCanExecuteOkButtonChanged(bool value)
    {
        Messenger.Send<ShellDialogCommandRequerySuggestedMessage>();
    }

    partial void OnFromDateChanged(DateOnly? value)
    {
        Revalidate();
    }

    private void OnRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BankFeedImportAccountRowViewModel.IsSelected))
        {
            Revalidate();
        }
    }

    private void OnShellDialogResponseReceived(ShellDialogResponseMessage message)
    {
        if (!message.IsItForMe(this.dialogCorrelationId) || this.pendingRequest is null)
        {
            return;
        }

        var request = message.Response == ShellDialogButton.Cancel ? null : CreateRequest();
        var pending = this.pendingRequest;
        this.pendingRequest = null;

        // Cancel the bank feed check before completing the request, it is no longer needed once the user has responded.
        this.feedDetailsCancellation?.Cancel();
        pending.TrySetResult(request);
    }

    partial void OnToDateChanged(DateOnly? value)
    {
        Revalidate();
    }

    private async Task RefreshAsync()
    {
        // Closing the dialog cancels the wait.
        var cancellationToken = this.feedDetailsCancellation?.Token ?? CancellationToken.None;
        var dataAsOfBefore = AccountRows
            .Where(r => r.FeedAccount is { IsActive: true })
            .ToDictionary(r => r.Mapping.ExternalAccountId, r => r.FeedAccount!.DataAsOf);

        IsRefreshing = true;
        RefreshStatus = "Asking the bank feed to refresh...";
        Revalidate();
        try
        {
            if (!await this.configurationService.RequestFeedRefreshAsync(cancellationToken))
            {
                // An earlier refresh may have finished since the dialog opened, so show the latest times.
                ApplyFeedAccounts(await this.configurationService.GetFeedAccountsAsync(cancellationToken));
                RefreshStatus = "The bank feed was asked to refresh only a few minutes ago, so it has not started another. The times shown are its latest.";
                return;
            }

            // The bank feed refreshes in the background, the only sign it has finished is each account's "data as of" time moving forward.
            RefreshStatus = "Waiting for the bank feed to refresh from your bank. This can take a minute...";
            for (var check = 1; check <= MaxRefreshChecks; check++)
            {
                await Task.Delay(RefreshCheckInterval, cancellationToken);
                var feedAccounts = await this.configurationService.GetFeedAccountsAsync(cancellationToken);
                ApplyFeedAccounts(feedAccounts);
                if (dataAsOfBefore.All(before => HasRefreshed(before.Value, feedAccounts.FirstOrDefault(a => a.ExternalId == before.Key))))
                {
                    RefreshStatus = "The bank feed has refreshed.";
                    return;
                }
            }

            RefreshStatus = "The bank feed has not reported new data yet. It may still be refreshing, or it may have skipped the refresh because its data was refreshed recently. "
                            + "Press Refresh again in a few minutes to check.";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The user closed the dialog while waiting. There is nothing to show.
            RefreshStatus = string.Empty;
        }
        catch (BankFeedAuthenticationException)
        {
            RefreshStatus = string.Empty;
            LoadError = "Your bank feed credentials were rejected. Update them on the Settings tab.";
        }
        catch (BankFeedException ex)
        {
            this.logger.LogWarning(_ => $"Unable to refresh the bank feed: {ex.Message}");
            RefreshStatus = $"Unable to refresh the bank feed: {ex.Message}";
        }
        finally
        {
            IsRefreshing = false;
            Revalidate();
        }
    }

    internal BankFeedImportRequest? CreateRequest()
    {
        if (!CanExecuteOkButton || FromDate is null || ToDate is null)
        {
            return null;
        }

        return new BankFeedImportRequest(
            new ImportDateRange(FromDate.Value, ToDate.Value),
            AccountRows.Where(r => r.IsSelected && r.IsAvailable).Select(r => r.Mapping).ToList());
    }

    private void UpdateWarnings()
    {
        var selected = AccountRows.Where(r => r.IsSelected && r.IsAvailable).ToList();

        // The suggested start may already overlap the latest day(s) on purpose, so the warning is only for a From date earlier than the suggestion.
        var overlapping = FromDate is null || (this.suggestedRange.Start is { } suggestedStart && FromDate.Value >= suggestedStart)
            ? []
            : selected
                .Where(r => this.existingTransactions.Any(t => t.Account.Name == r.AccountName && t.Date >= FromDate.Value))
                .Select(r => r.AccountName)
                .Distinct()
                .ToList();
        OverlapWarning = overlapping.Count == 0
            ? string.Empty
            : $"This range overlaps transactions you already have for {string.Join(", ", overlapping)}. Transactions previously imported from a CSV file won't be recognised as duplicates; review the list after importing.";

        // Data refreshed before the end of the To day may be missing that day's transactions.
        var endOfToDay = ToDate?.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        foreach (var row in AccountRows)
        {
            row.IsStale = endOfToDay is not null && row.FeedAccount?.DataAsOf is { } asOf && asOf.LocalDateTime < endOfToDay.Value;
        }

        var stale = selected.Where(r => r.IsStale).Select(r => r.FeedAccount!.Institution).Distinct().ToList();
        StaleWarning = stale.Count == 0
            ? string.Empty
            : $"{string.Join(", ", stale)} data was last refreshed before the end of your To date. Recent transactions may be missing; press Refresh, or consider an earlier To date.";
    }

    private string Validate()
    {
        if (FromDate is null)
        {
            return "Choose a From date.";
        }

        if (ToDate is null)
        {
            return "Choose a To date.";
        }

        if (FromDate.Value > ToDate.Value)
        {
            return this.suggestedRange.IsEmpty && FromDate == this.suggestedRange.Start && ToDate == this.suggestedRange.End
                ? string.Format(CultureInfo.CurrentCulture, "You're up to date. The next download is available after {0:d}.", this.suggestedRange.Start?.AddDays(this.suggestedStartOverlapDays))
                : "From must be on or before To.";
        }

        if (ToDate.Value >= Today)
        {
            return "To must be before today, because today's transactions are not final.";
        }

        if (this.historyAvailableFrom is not null && FromDate.Value < this.historyAvailableFrom.Value)
        {
            return string.Format(CultureInfo.CurrentCulture, "The bank feed only has transactions from {0:d}.", this.historyAvailableFrom.Value);
        }

        if (AccountRows.All(r => !(r.IsSelected && r.IsAvailable)))
        {
            return "Tick at least one account.";
        }

        return string.Empty;
    }
}

/// <summary>
///     What the user chose in the "Download from bank feed" dialog.
/// </summary>
public record BankFeedImportRequest(ImportDateRange Range, IReadOnlyList<BankFeedAccountMapping> Mappings);
