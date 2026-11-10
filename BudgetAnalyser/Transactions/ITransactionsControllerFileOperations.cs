namespace BudgetAnalyser.Transactions;

public interface ITransactionsControllerFileOperations
{
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
