using System.Collections.ObjectModel;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.BankFeed;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;
using Rees.Wpf.Contracts;

namespace BudgetAnalyser.Settings;

/// <summary>
///     Drives the two bank feed cards on the Settings tab. "Bank feed connection" holds the credentials, which are stored encrypted for this Windows user and saved
///     immediately. "Bank feed accounts" holds the account mappings, which are stored in the open budget file: edits are kept here until Apply, then saved with the file.
///     Nothing here names a vendor; the provider supplies its own display name and credential fields.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public partial class BankFeedSettingsController : ControllerBase
{
    internal const int PreviewCount = 10;

    private readonly IAccountTypeRepository accountTypeRepository;
    private readonly IBankFeedConfigurationService configurationService;
    private readonly ILogger logger;
    private readonly IUserMessageBox messageBox;
    private readonly IBankFeedPreviewService previewService;
    private readonly IUserQuestionBoxYesNo questionBox;
    private IReadOnlyList<BankFeedAccount>? feedAccounts;
    private bool? lastBudgetFileOpen;
    private IReadOnlyList<BankFeedAccountMapping>? lastMappings;

    public BankFeedSettingsController(
        IMessenger messenger,
        IBankFeedConfigurationService configurationService,
        IBankFeedPreviewService previewService,
        IAccountTypeRepository accountTypeRepository,
        UserPrompts userPrompts,
        ILogger logger) : base(messenger)
    {
        ArgumentNullException.ThrowIfNull(userPrompts);
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.previewService = previewService ?? throw new ArgumentNullException(nameof(previewService));
        this.accountTypeRepository = accountTypeRepository ?? throw new ArgumentNullException(nameof(accountTypeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.messageBox = userPrompts.MessageBox ?? throw new ArgumentNullException(nameof(userPrompts.MessageBox));
        this.questionBox = userPrompts.YesNoBox ?? throw new ArgumentNullException(nameof(userPrompts.YesNoBox));

        SaveCredentialsCommand = new RelayCommand(OnSaveCredentials, () => CredentialRows.All(r => !string.IsNullOrWhiteSpace(r.NewValue)));
        ClearCredentialsCommand = new RelayCommand(OnClearCredentials, () => CredentialRows.Any(r => r.HasStoredValue));
        TestConnectionCommand = new AsyncRelayCommand(OnTestConnectionAsync, CanUseFeed);
        RefreshAccountsCommand = new AsyncRelayCommand(OnRefreshAccountsAsync, () => CanUseFeed() && IsBudgetFileOpen);
        ApplyMappingsCommand = new RelayCommand(OnApplyMappings, () => HasUnappliedChanges && IsBudgetFileOpen);
        RevertMappingsCommand = new RelayCommand(OnRevertMappings, () => HasUnappliedChanges);
        PreviewCommand = new AsyncRelayCommand<BankFeedAccountRowViewModel>(OnPreviewAsync, row => row is { IsMapped: true, IsPreviewing: false } && CanUseFeed());
        RemoveMappingCommand = new RelayCommand<BankFeedAccountRowViewModel>(OnRemoveMapping, row => row is not null);

        this.configurationService.ConfigurationChanged += OnConfigurationChanged;
        RefreshFromConfiguration();
    }

    public ObservableCollection<BankFeedAccountRowViewModel> AccountRows { get; } = new();

    public IRelayCommand ApplyMappingsCommand { get; }

    public IRelayCommand ClearCredentialsCommand { get; }

    [ObservableProperty]
    public partial string ConnectionStatus { get; private set; } = string.Empty;

    public ObservableCollection<BankFeedCredentialRowViewModel> CredentialRows { get; } = new();

    [ObservableProperty]
    public partial bool HasUnappliedChanges { get; private set; }

    [ObservableProperty]
    public partial bool IsBudgetFileOpen { get; private set; }

    [ObservableProperty]
    public partial bool IsBusy { get; private set; }

    [ObservableProperty]
    public partial bool IsConfigured { get; private set; }

    [ObservableProperty]
    public partial string MappingStatus { get; private set; } = string.Empty;

    public IAsyncRelayCommand<BankFeedAccountRowViewModel> PreviewCommand { get; }

    public string ProviderDisplayName => this.configurationService.ProviderDisplayName;

    public string ProviderSetupInstructionsText => this.configurationService.ProviderSetupInstructionsText;

    public Uri? ProviderSetupInstructionsUrl => this.configurationService.ProviderSetupInstructionsUrl;

    public IAsyncRelayCommand RefreshAccountsCommand { get; }

    public IRelayCommand<BankFeedAccountRowViewModel> RemoveMappingCommand { get; }

    public IRelayCommand RevertMappingsCommand { get; }

    public IRelayCommand SaveCredentialsCommand { get; }

    public IAsyncRelayCommand TestConnectionCommand { get; }

    private static string DescribeFailure(BankFeedException ex)
    {
        return ex is BankFeedAuthenticationException
            ? "Your bank feed credentials were rejected. Enter them again and press Save, then Test connection."
            : $"Unable to reach the bank feed: {ex.Message}";
    }

    private bool CanUseFeed()
    {
        return !IsBusy && CredentialRows.Count > 0 && CredentialRows.All(r => r.HasStoredValue);
    }

    private IReadOnlyList<BankFeedAccountChoice> CreateAccountChoices()
    {
        return new[] { new BankFeedAccountChoice(null) }
            .Concat(this.accountTypeRepository.ListCurrentlyUsedAccountTypes().OrderBy(a => a.Name).Select(a => new BankFeedAccountChoice(a)))
            .ToList();
    }

    private void NotifyCommands()
    {
        SaveCredentialsCommand.NotifyCanExecuteChanged();
        ClearCredentialsCommand.NotifyCanExecuteChanged();
        TestConnectionCommand.NotifyCanExecuteChanged();
        RefreshAccountsCommand.NotifyCanExecuteChanged();
        ApplyMappingsCommand.NotifyCanExecuteChanged();
        RevertMappingsCommand.NotifyCanExecuteChanged();
        PreviewCommand.NotifyCanExecuteChanged();
    }

    private void OnApplyMappings()
    {
        var problem = AccountRows.Select(r => r.Validate()).FirstOrDefault(p => p is not null);
        if (problem is not null)
        {
            this.messageBox.Show(problem, "Bank feed accounts");
            return;
        }

        var mappings = AccountRows
            .Where(r => r.SelectedAccount.Account is not null)
            .Select(r => new BankFeedAccountMapping(r.ExternalAccountId, r.SelectedAccount.Account!, r.ToFieldMapping()))
            .ToList();
        // Raises ConfigurationChanged, which rebuilds the rows from the applied mappings.
        this.configurationService.SaveMappings(mappings);
        MappingStatus = "Applied. Save your budget file to keep these changes.";
    }

    private void OnClearCredentials()
    {
        var confirmation = this.questionBox.Show(
            $"Are you sure you want to remove the saved {ProviderDisplayName} credentials from this computer? You will need to enter them again to download transactions.",
            "Clear bank feed credentials?");
        if (confirmation is not true)
        {
            return;
        }

        this.configurationService.ClearCredentials();
        ForgetFeedAccounts();
        ConnectionStatus = "Credentials removed.";
    }

    /// <summary>
    ///     Forgets the account details loaded from the bank feed, because the credentials they were loaded with have been changed or removed. Accounts that are not mapped are
    ///     removed from the list, as they are only known from the feed. Mapped accounts stay, without their feed details, so unapplied edits are never discarded.
    /// </summary>
    private void ForgetFeedAccounts()
    {
        this.feedAccounts = null;
        foreach (var row in AccountRows.Where(r => !r.IsMapped).ToList())
        {
            row.Edited -= OnRowEdited;
            AccountRows.Remove(row);
        }

        foreach (var row in AccountRows)
        {
            row.ClearFeedDetails();
        }

        NotifyCommands();
    }

    private void OnConfigurationChanged(object? sender, EventArgs e)
    {
        RefreshFromConfiguration();
    }

    private void OnCredentialRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        SaveCredentialsCommand.NotifyCanExecuteChanged();
    }

    private void OnRowEdited(object? sender, EventArgs e)
    {
        HasUnappliedChanges = true;
        MappingStatus = "You have changes that have not been applied.";
        NotifyCommands();
    }

    private async Task OnPreviewAsync(BankFeedAccountRowViewModel? row)
    {
        if (row?.SelectedAccount.Account is null)
        {
            return;
        }

        row.IsPreviewing = true;
        row.PreviewTransactions.Clear();
        row.PreviewMessage = "Downloading...";
        PreviewCommand.NotifyCanExecuteChanged();
        try
        {
            var mapping = new BankFeedAccountMapping(row.ExternalAccountId, row.SelectedAccount.Account, row.ToFieldMapping());
            var transactions = await this.previewService.PreviewAsync(mapping, DateOnlyExt.Today(), PreviewCount, CancellationToken.None);
            foreach (var transaction in transactions)
            {
                row.PreviewTransactions.Add(transaction);
            }

            row.PreviewMessage = transactions.Count == 0
                ? $"No transactions in the last {IBankFeedPreviewService.PreviewDays} days."
                : "Preview only, nothing has been imported.";
        }
        catch (BankFeedException ex)
        {
            this.logger.LogWarning(_ => $"Bank feed preview failed: {ex.Message}");
            row.PreviewMessage = DescribeFailure(ex);
        }
        finally
        {
            row.IsPreviewing = false;
            PreviewCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task OnRefreshAccountsAsync()
    {
        IsBusy = true;
        NotifyCommands();
        try
        {
            this.feedAccounts = await this.configurationService.GetFeedAccountsAsync(CancellationToken.None);
            if (HasUnappliedChanges)
            {
                MappingStatus = "Account details loaded. Your unapplied changes were kept; press Revert to see the updated details.";
            }
            else
            {
                RebuildAccountRows();
                MappingStatus = string.Empty;
            }
        }
        catch (BankFeedException ex)
        {
            this.logger.LogWarning(_ => $"Bank feed account refresh failed: {ex.Message}");
            MappingStatus = DescribeFailure(ex);
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }
    }

    private void OnRevertMappings()
    {
        RebuildAccountRows();
        MappingStatus = string.Empty;
    }

    private void OnRemoveMapping(BankFeedAccountRowViewModel? row)
    {
        if (row is null)
        {
            return;
        }

        row.Edited -= OnRowEdited;
        AccountRows.Remove(row);
        OnRowEdited(this, EventArgs.Empty);
    }

    private void OnSaveCredentials()
    {
        var values = CredentialRows.ToDictionary(r => r.Key, r => r.NewValue.Trim());
        // Raises ConfigurationChanged, which rebuilds the credential rows so the typed values are discarded.
        this.configurationService.SaveCredentials(values);
        ForgetFeedAccounts();
        ConnectionStatus = "Credentials saved. Press Test connection to check them.";
    }

    private async Task OnTestConnectionAsync()
    {
        IsBusy = true;
        ConnectionStatus = "Testing connection...";
        NotifyCommands();
        try
        {
            var status = await this.configurationService.TestConnectionAsync(CancellationToken.None);
            ConnectionStatus = status.IsConnected
                ? $"Connected as {status.ConnectedAs ?? "(unknown)"}."
                : "The bank feed did not confirm the connection.";
        }
        catch (BankFeedException ex)
        {
            this.logger.LogWarning(_ => $"Bank feed connection test failed: {ex.Message}");
            ConnectionStatus = DescribeFailure(ex);
            return;
        }
        finally
        {
            IsBusy = false;
            NotifyCommands();
        }

        if (IsBudgetFileOpen)
        {
            await OnRefreshAccountsAsync();
        }
    }

    private void RebuildAccountRows()
    {
        foreach (var row in AccountRows)
        {
            row.Edited -= OnRowEdited;
        }

        AccountRows.Clear();
        HasUnappliedChanges = false;
        if (!IsBudgetFileOpen)
        {
            NotifyCommands();
            return;
        }

        var choices = CreateAccountChoices();
        var mappings = this.configurationService.Mappings;
        foreach (var mapping in mappings)
        {
            var feedAccount = this.feedAccounts?.FirstOrDefault(a => a.ExternalId == mapping.ExternalAccountId);
            bool? found = this.feedAccounts is null ? null : feedAccount is not null;
            var choice = choices.FirstOrDefault(c => c.Account?.Name == mapping.Account.Name) ?? new BankFeedAccountChoice(mapping.Account);
            AddRow(new BankFeedAccountRowViewModel(mapping.ExternalAccountId, feedAccount, found, choices, choice, mapping.FieldMapping));
        }

        foreach (var feedAccount in this.feedAccounts?.Where(a => mappings.All(m => m.ExternalAccountId != a.ExternalId)) ?? [])
        {
            AddRow(new BankFeedAccountRowViewModel(
                feedAccount.ExternalId, feedAccount, true, choices, choices[0], BankFeedFieldMappingPresets.ForInstitution(feedAccount.Institution)));
        }

        NotifyCommands();
    }

    private void AddRow(BankFeedAccountRowViewModel row)
    {
        row.Edited += OnRowEdited;
        row.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(BankFeedAccountRowViewModel.SelectedAccount))
            {
                PreviewCommand.NotifyCanExecuteChanged();
            }
        };
        AccountRows.Add(row);
    }

    private void RebuildCredentialRows()
    {
        foreach (var row in CredentialRows)
        {
            row.PropertyChanged -= OnCredentialRowPropertyChanged;
        }

        CredentialRows.Clear();
        foreach (var field in this.configurationService.CredentialFields)
        {
            var row = new BankFeedCredentialRowViewModel(field, this.configurationService.HasStoredValue(field.Key));
            row.PropertyChanged += OnCredentialRowPropertyChanged;
            CredentialRows.Add(row);
        }
    }

    private void RefreshFromConfiguration()
    {
        RebuildCredentialRows();
        IsConfigured = this.configurationService.IsConfigured;
        IsBudgetFileOpen = this.configurationService.IsBudgetFileOpen;

        // Only rebuild the account rows when the mappings or the open file have changed, so a credential change never discards unapplied edits.
        if (!ReferenceEquals(this.lastMappings, this.configurationService.Mappings) || this.lastBudgetFileOpen != IsBudgetFileOpen)
        {
            this.lastMappings = this.configurationService.Mappings;
            this.lastBudgetFileOpen = IsBudgetFileOpen;
            RebuildAccountRows();
        }

        NotifyCommands();
    }
}
