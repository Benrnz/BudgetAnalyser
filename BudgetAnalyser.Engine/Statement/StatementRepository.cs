using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
///     it is loaded with no prompting, otherwise the user is prompted for a filename.
///     It also is responsible for saving  any open statement file into a budget analyser statement file.
///     To function it orchestrates across the  <see cref="IVersionedStatementModelRepository" /> and the
///     <see cref="IBankStatementImporterRepository" />.
///     This implementation is strictly not thread safe and should be single threaded only.  Don't allow multiple threads
///     to use it at the same time.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used by IoC")]
[AutoRegisterWithIoC(SingleInstance = true)]
internal class StatementRepository(IVersionedStatementModelRepository statementModelRepository, IBankStatementImporterRepository importerRepository)
    : IStatementRepository
{
    private readonly IBankStatementImporterRepository importerRepository = importerRepository ?? throw new ArgumentNullException(nameof(importerRepository));
    private readonly IVersionedStatementModelRepository statementModelRepository = statementModelRepository ?? throw new ArgumentNullException(nameof(statementModelRepository));

    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        await this.statementModelRepository.CreateNewAndSaveAsync(storageKey);
    }

    public async Task<TransactionSetModel> ImportBankStatementAsync(
        string storageKey,
        Account account)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        return account is null
            ? throw new ArgumentNullException(nameof(account))
            : await this.importerRepository.ImportAsync(storageKey, account);
    }

    public async Task<TransactionSetModel> LoadAsync(string storageKey, bool isEncrypted)
    {
        return storageKey is null
            ? throw new FileNotFoundException("storageKey")
            : await this.statementModelRepository.LoadAsync(storageKey, isEncrypted);
    }

    public async Task SaveAsync(TransactionSetModel transactionSetModel, bool isEncrypted)
    {
        if (transactionSetModel is null)
        {
            throw new ArgumentNullException(nameof(transactionSetModel));
        }

        await this.statementModelRepository.SaveAsync(transactionSetModel, isEncrypted);
    }
}
