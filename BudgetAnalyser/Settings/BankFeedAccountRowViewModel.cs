using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using BudgetAnalyser.Engine.BankFeed;
using BudgetAnalyser.Engine.Transactions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetAnalyser.Settings;

/// <summary>
///     One bank feed account on the "Bank feed accounts" card: which Budget Analyser account it imports into, and how its fields are mapped.
///     Edits stay here until the user presses Apply.
/// </summary>
public partial class BankFeedAccountRowViewModel : ObservableObject
{
    private static readonly HashSet<string> EditableProperties =
    [
        nameof(SelectedAccount),
        nameof(DescriptionSource), nameof(DescriptionFallback),
        nameof(Reference1Source), nameof(Reference1Fallback),
        nameof(Reference2Source), nameof(Reference2Fallback),
        nameof(Reference3Source), nameof(Reference3Fallback),
        nameof(TypeSource), nameof(TypeFallback)
    ];

    /// <summary>
    ///     Creates a row.
    /// </summary>
    /// <param name="externalAccountId">The provider's account identifier.</param>
    /// <param name="feedAccount">The account's details from the bank feed, or null if they haven't been loaded or the feed no longer has the account.</param>
    /// <param name="foundInFeed">True if the feed has the account, false if it doesn't, null if the feed hasn't been checked yet.</param>
    /// <param name="accountChoices">The choices for "Import into".</param>
    /// <param name="selectedAccount">The current choice.</param>
    /// <param name="fieldMapping">The field mapping to start with.</param>
    public BankFeedAccountRowViewModel(
        string externalAccountId,
        BankFeedAccount? feedAccount,
        bool? foundInFeed,
        IReadOnlyList<BankFeedAccountChoice> accountChoices,
        BankFeedAccountChoice selectedAccount,
        BankFeedFieldMapping fieldMapping)
    {
        ExternalAccountId = externalAccountId;
        FeedAccount = feedAccount;
        FoundInFeed = foundInFeed;
        AccountChoices = accountChoices;
        SelectedAccount = selectedAccount;
        DescriptionSource = Pick(fieldMapping.Description, 0);
        DescriptionFallback = Pick(fieldMapping.Description, 1);
        Reference1Source = Pick(fieldMapping.Reference1, 0);
        Reference1Fallback = Pick(fieldMapping.Reference1, 1);
        Reference2Source = Pick(fieldMapping.Reference2, 0);
        Reference2Fallback = Pick(fieldMapping.Reference2, 1);
        Reference3Source = Pick(fieldMapping.Reference3, 0);
        Reference3Fallback = Pick(fieldMapping.Reference3, 1);
        TypeSource = Pick(fieldMapping.TransactionType, 0);
        TypeFallback = Pick(fieldMapping.TransactionType, 1);
        foreach (var pair in fieldMapping.TypeNames)
        {
            AddTypeNameRow(new BankFeedTypeNameRowViewModel { FeedType = pair.Key, ImportAs = pair.Value });
        }

        TypeNames.CollectionChanged += OnTypeNamesCollectionChanged;
        PreviewTransactions.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasPreviewTransactions));
        PropertyChanged += OnOwnPropertyChanged;
    }

    /// <summary>
    ///     Raised whenever the user changes anything that is saved by Apply.
    /// </summary>
    public event EventHandler? Edited;

    public IReadOnlyList<BankFeedAccountChoice> AccountChoices { get; }

    public string AccountNumber => FeedAccount?.AccountNumber ?? string.Empty;

    public string DataAsOfText => FeedAccount?.DataAsOf is { } asOf
        ? string.Format(CultureInfo.CurrentCulture, "Data as of {0:g}", asOf.ToLocalTime())
        : string.Empty;

    [ObservableProperty]
    public partial BankFeedSourceField DescriptionFallback { get; set; }

    [ObservableProperty]
    public partial BankFeedSourceField DescriptionSource { get; set; }

    public string ExternalAccountId { get; }

    public BankFeedAccount? FeedAccount { get; }

    public bool? FoundInFeed { get; }

    public bool HasPreviewTransactions => PreviewTransactions.Count > 0;

    public bool IsInactive => FeedAccount is { IsActive: false };

    public bool IsMapped => SelectedAccount.Account is not null;

    public bool IsNotFoundInFeed => FoundInFeed == false;

    [ObservableProperty]
    public partial bool IsPreviewing { get; set; }

    [ObservableProperty]
    public partial string PreviewMessage { get; set; } = string.Empty;

    public ObservableCollection<Transaction> PreviewTransactions { get; } = new();

    [ObservableProperty]
    public partial BankFeedSourceField Reference1Fallback { get; set; }

    [ObservableProperty]
    public partial BankFeedSourceField Reference1Source { get; set; }

    [ObservableProperty]
    public partial BankFeedSourceField Reference2Fallback { get; set; }

    [ObservableProperty]
    public partial BankFeedSourceField Reference2Source { get; set; }

    [ObservableProperty]
    public partial BankFeedSourceField Reference3Fallback { get; set; }

    [ObservableProperty]
    public partial BankFeedSourceField Reference3Source { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMapped))]
    public partial BankFeedAccountChoice SelectedAccount { get; set; }

    public IReadOnlyList<BankFeedSourceFieldOption> SourceFields => BankFeedSourceFieldOption.All;

    public string StatusText => FoundInFeed switch
    {
        null => "Account details have not been loaded. Press Refresh accounts.",
        false => "Account not found in bank feed.",
        true when IsInactive => "This account is not currently connected in the bank feed. Reconnect it with your bank feed provider.",
        true => string.Empty
    };

    public string Title => FeedAccount is null ? ExternalAccountId : $"{FeedAccount.Institution}  {FeedAccount.Name}";

    [ObservableProperty]
    public partial BankFeedSourceField TypeFallback { get; set; }

    public ObservableCollection<BankFeedTypeNameRowViewModel> TypeNames { get; } = new();

    [ObservableProperty]
    public partial BankFeedSourceField TypeSource { get; set; }

    [RelayCommand]
    public void AddTypeName()
    {
        AddTypeNameRow(new BankFeedTypeNameRowViewModel());
    }

    [RelayCommand]
    public void RemoveTypeName(BankFeedTypeNameRowViewModel? row)
    {
        if (row is not null)
        {
            TypeNames.Remove(row);
        }
    }

    /// <summary>
    ///     Builds the field mapping from the editors. Each target uses the source, then the fallback, ignoring "(none)". Blank type name entries are ignored.
    /// </summary>
    public BankFeedFieldMapping ToFieldMapping()
    {
        var typeNames = new Dictionary<string, string>();
        foreach (var row in TypeNames.Where(r => !string.IsNullOrWhiteSpace(r.FeedType)))
        {
            typeNames[row.FeedType.Trim()] = row.ImportAs.Trim();
        }

        return new BankFeedFieldMapping(
            Sources(DescriptionSource, DescriptionFallback),
            Sources(Reference1Source, Reference1Fallback),
            Sources(Reference2Source, Reference2Fallback),
            Sources(Reference3Source, Reference3Fallback),
            Sources(TypeSource, TypeFallback),
            typeNames);
    }

    /// <summary>
    ///     Returns a description of the first problem that stops this row being applied, or null if there is none.
    /// </summary>
    public string? Validate()
    {
        var duplicate = TypeNames
            .Where(r => !string.IsNullOrWhiteSpace(r.FeedType))
            .GroupBy(r => r.FeedType.Trim())
            .FirstOrDefault(g => g.Count() > 1);
        return duplicate is null ? null : $"The type '{duplicate.Key}' is listed more than once in the type names for {Title}.";
    }

    private static BankFeedSourceField Pick(IReadOnlyList<BankFeedSourceField> sources, int index)
    {
        return index < sources.Count ? sources[index] : BankFeedSourceField.None;
    }

    private static List<BankFeedSourceField> Sources(BankFeedSourceField source, BankFeedSourceField fallback)
    {
        return new[] { source, fallback }.Where(f => f != BankFeedSourceField.None).Distinct().ToList();
    }

    private void AddTypeNameRow(BankFeedTypeNameRowViewModel row)
    {
        row.PropertyChanged += OnTypeNameRowPropertyChanged;
        TypeNames.Add(row);
    }

    private void OnOwnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not null && EditableProperties.Contains(e.PropertyName))
        {
            Edited?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnTypeNameRowPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Edited?.Invoke(this, EventArgs.Empty);
    }

    private void OnTypeNamesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var removed in e.OldItems?.OfType<BankFeedTypeNameRowViewModel>() ?? [])
        {
            removed.PropertyChanged -= OnTypeNameRowPropertyChanged;
        }

        Edited?.Invoke(this, EventArgs.Empty);
    }
}
