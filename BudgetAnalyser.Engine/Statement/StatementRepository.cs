using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     The Statement File Manager is responsible for loading any kind of statement file, whether it be an existing budget
    ///     analyser statement file or a downloaded bank statement export to merge or load. If the filename is already known
    ///     it is loaded with no prompting, otherwise the user is prompted for a filename.
    ///     It also is responsible for saving  any open statement file into a budget analyser statement file.
    ///     To function it orchestrates across the  <see cref="IVersionedStatementModelRepository" /> and the
    ///     <see cref="IBankStatementImporterRepository" />.
    ///     This implementation is strictly not thread safe and should be single threaded only.  Don't allow mulitple threads
    ///     to use it at the same time.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used by IoC")]
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class StatementRepository : IStatementRepository
    {
        private readonly IBankStatementImporterRepository importerRepository;
        private readonly IVersionedStatementModelRepository statementModelRepository;

        public StatementRepository(
            [NotNull] IVersionedStatementModelRepository statementModelRepository,
            [NotNull] IBankStatementImporterRepository importerRepository)
        {
            if (statementModelRepository == null)
            {
                throw new ArgumentNullException("statementModelRepository");
            }

            if (importerRepository == null)
            {
                throw new ArgumentNullException("importerRepository");
            }
            this.statementModelRepository = statementModelRepository;
            this.importerRepository = importerRepository;
        }

        public StatementModel ImportAndMergeBankStatementAsync(
            string storageKey,
            AccountType account)
        {
            if (storageKey == null)
            {
                throw new ArgumentNullException("storageKey");
            }

            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            // TODO add generic UI to let user classify columns.
            // TODO this should be async also
            return this.importerRepository.Import(storageKey, account);
        }

        public async Task<StatementModel> LoadStatementModelAsync(string storageKey)
        {
            if (storageKey == null)
            {
                throw new ArgumentNullException("storageKey");
            }

            // TODO add generic UI to let user classify columns.
            return await this.statementModelRepository.LoadAsync(storageKey);
        }

        public async Task SaveAsync([NotNull] StatementModel statementModel)
        {
            if (statementModel == null)
            {
                throw new ArgumentNullException("statementModel");
            }

            await this.statementModelRepository.SaveAsync(statementModel, statementModel.StorageKey);
        }
    }
}