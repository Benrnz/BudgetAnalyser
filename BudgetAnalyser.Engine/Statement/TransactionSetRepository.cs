using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     The Transactions-Model File Manager is responsible for loading and saving the BudgetAnalyser transactions-model file. It also is responsible for importing new bank statement exported files.
///     To function it orchestrates across the  <see cref="IVersionedStatementModelRepository" /> and the <see cref="IBankStatementImporterRepository" />. This implementation is strictly not thread
///     safe and should be single threaded only.  Don't allow multiple threads to use it at the same time.
/// </summary>
[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used by IoC")]
[AutoRegisterWithIoC(SingleInstance = true)]
internal class TransactionSetRepository(IVersionedStatementModelRepository statementModelRepository, IBankStatementImporterRepository importerRepository)
    : ITransactionSetRepository
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
