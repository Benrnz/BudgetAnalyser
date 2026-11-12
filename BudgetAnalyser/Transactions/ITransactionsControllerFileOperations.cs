using CommunityToolkit.Mvvm.Input;

namespace BudgetAnalyser.Transactions;

public interface ITransactionsControllerFileOperations
{
    /// <summary>
    ///     Cancels the bank feed download that is in progress. Nothing is imported. Only enabled while <see cref="IsDownloadingFromBankFeed" /> is true.
    /// </summary>
    IRelayCommand CancelBankFeedDownloadCommand { get; }

    /// <summary>
    ///     True while transactions are being downloaded from the bank feed, and the download can still be cancelled.
    /// </summary>
    bool IsDownloadingFromBankFeed { get; }

    bool LoadingData { get; }

    TransactionsListViewModel ViewModel { get; }

    void Close();

    /// <summary>
    ///     Saves any unsaved changes, asks the user what to download from the bank feed, then downloads and merges it, and shows a summary of what was imported.
    /// </summary>
    Task MergeInBankFeedTransactions();

    // TODO Should this be renamed now there are two ways to import transactions?
    Task MergeInNewTransactions();
    void NotifyOfEdit();
    Task SyncWithServiceAsync();
}
