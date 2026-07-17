namespace BudgetAnalyser.Transactions;

public interface ITransactionsControllerFileOperations
{
    bool LoadingData { get; }

    TransactionsListViewModel ViewModel { get; }

    void Close();

    Task MergeInNewTransactions();
    void NotifyOfEdit();
    Task SyncWithServiceAsync();
}
