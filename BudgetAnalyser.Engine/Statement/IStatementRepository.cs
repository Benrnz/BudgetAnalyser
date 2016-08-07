using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     The Statement Repository is responsible for loading <see cref="StatementModel" />s and Bank Extracts for the
    ///     purpose of merging with an existing <see cref="StatementModel" />.
    ///     It also is responsible for saving <see cref="StatementModel" />s.
    ///     To function it orchestrates across the <see cref="IVersionedStatementModelRepository" /> and the
    ///     <see cref="IBankStatementImporterRepository" />.
    /// </summary>
    public interface IStatementRepository
    {
        /// <summary>
        ///     Creates a new empty <see cref="StatementModel" /> at the location indicated by the <paramref name="storageKey" />.
        ///     Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to load the new collection.
        /// </summary>
        Task CreateNewAndSaveAsync(string storageKey);

        /// <summary>
        ///     Imports a bank's transaction extract and returns it as a new <see cref="StatementModel" />.  This can then be
        ///     merged with another <see cref="StatementModel" /> using the
        ///     <see cref="StatementModel.Merge(BudgetAnalyser.Engine.Statement.StatementModel)" /> method.
        /// </summary>
        /// <exception cref="NotSupportedException">Will be thrown if the format of the bank extract is not supported.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Will be thrown if the bank extract cannot be located using the given
        ///     <paramref name="storageKey" />
        /// </exception>
        Task<StatementModel> ImportBankStatementAsync(
            [NotNull] string storageKey,
            [NotNull] Account account);

        /// <summary>
        ///     Loads an existing Budget Analyser Transaction file.
        /// </summary>
        /// <param name="storageKey">Pass a known storage key (database identifier or filename) to load.</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <exception cref="NotSupportedException">Will be thrown if the format of the bank extract is not supported.</exception>
        /// <exception cref="KeyNotFoundException">
        ///     Will be thrown if the bank extract cannot be located using the given <paramref name="storageKey" />
        /// </exception>
        /// <exception cref="StatementModelChecksumException">
        ///     Will be thrown if the statement model's internal checksum detects corrupt data indicating tampering.
        /// </exception>
        /// <exception cref="DataFormatException">
        ///     Will be thrown if the format of the bank extract contains unexpected data indicating it is corrupt or an old file.
        /// </exception>
        Task<StatementModel> LoadAsync([NotNull] string storageKey, bool isEncrypted);

        /// <summary>
        ///     Save the given <see cref="StatementModel" /> into persistent storage. Files are saved into the proprietry
        ///     Budget Analyser CSV format.
        /// </summary>
        /// <param name="statementModel">The model to save.</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        Task SaveAsync(StatementModel statementModel, bool isEncrypted);
    }
}