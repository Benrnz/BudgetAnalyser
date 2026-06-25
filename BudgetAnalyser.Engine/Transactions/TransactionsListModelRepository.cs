using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
///     it is loaded with no prompting, otherwise the user is prompted for a filename.
///     It also is responsible for saving  any open statement file into a budget analyser statement file.
///     To function it orchestrates across the  <see cref="IVersionedTransactionsModelRepository" /> and the
///     <see cref="IBankExtractImporterRepository" />.
///     This implementation is strictly not thread safe and should be single threaded only.  Don't allow multiple threads
///     to use it at the same time.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used by IoC")]
[AutoRegisterWithIoC(SingleInstance = true)]
internal class TransactionsListModelRepository(IVersionedTransactionsModelRepository transactionsListModelRepository, IBankExtractImporterRepository importerRepository)
    : ITransactionsListModelRepository
{
    private readonly IBankExtractImporterRepository importerRepository = importerRepository ?? throw new ArgumentNullException(nameof(importerRepository));
    private readonly IVersionedTransactionsModelRepository transactionsListModelRepository = transactionsListModelRepository ?? throw new ArgumentNullException(nameof(transactionsListModelRepository));

    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        await this.transactionsListModelRepository.CreateNewAndSaveAsync(storageKey);
    }

    public async Task<TransactionsListModel> ImportTransactionsExtractAsync(string storageKey, Account account)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        return account is null
            ? throw new ArgumentNullException(nameof(account))
            : await this.importerRepository.ImportAsync(storageKey, account);
    }

    public async Task<TransactionsListModel> LoadAsync(string storageKey, bool isEncrypted)
    {
        return storageKey is null
            ? throw new FileNotFoundException("storageKey")
            : await this.transactionsListModelRepository.LoadAsync(storageKey, isEncrypted);
    }

    public async Task SaveAsync(TransactionsListModel transactions, bool isEncrypted)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        await this.transactionsListModelRepository.SaveAsync(transactions, isEncrypted);
    }
}
