using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
///     it is loaded with no prompting, otherwise the user is prompted for a filename.
///     It also is responsible for saving  any open statement file into a budget analyser statement file.
///     To function it orchestrates across the  <see cref="IVersionedTransactionsModelRepository" /> and the
///     <see cref="IBankStatementImporterRepository" />.
///     This implementation is strictly not thread safe and should be single threaded only.  Don't allow multiple threads
///     to use it at the same time.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used by IoC")]
[AutoRegisterWithIoC(SingleInstance = true)]
internal class TransactionsListModelRepository(IVersionedTransactionsModelRepository statementModelRepository, IBankStatementImporterRepository importerRepository)
    : ITransactionsListModelRepository
{
    private readonly IBankStatementImporterRepository importerRepository = importerRepository ?? throw new ArgumentNullException(nameof(importerRepository));
    private readonly IVersionedTransactionsModelRepository statementModelRepository = statementModelRepository ?? throw new ArgumentNullException(nameof(statementModelRepository));

    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        await this.statementModelRepository.CreateNewAndSaveAsync(storageKey);
    }

    public async Task<TransactionsListModel> ImportBankStatementAsync(string storageKey, Account account)
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
            : await this.statementModelRepository.LoadAsync(storageKey, isEncrypted);
    }

    public async Task SaveAsync(TransactionsListModel transactions, bool isEncrypted)
    {
        if (transactions is null)
        {
            throw new ArgumentNullException(nameof(transactions));
        }

        await this.statementModelRepository.SaveAsync(transactions, isEncrypted);
    }
}
