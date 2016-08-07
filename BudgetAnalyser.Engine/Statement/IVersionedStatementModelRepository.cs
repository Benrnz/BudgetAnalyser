using System.Threading.Tasks;
using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     This interface describes persistence functions for retrieving and saving <see cref="StatementModel" />. It will
    ///     only load and save in the proprietry Budget Analyser Transaction format.
    ///     Rather than consuming this interface in a client, prefer to use the <see cref="ITransactionManagerService" />
    ///     instead.
    /// </summary>
    public interface IVersionedStatementModelRepository
    {
        /// <summary>
        ///     Creates a new empty <see cref="StatementModel" /> at the location indicated by the <paramref name="storageKey" />.
        ///     Any existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" />
        ///     method
        ///     to load the new <see cref="StatementModel" />.
        /// </summary>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <exception cref="System.ArgumentNullException">Will be thrown if any arguments are null.</exception>
        Task CreateNewAndSaveAsync([NotNull] string storageKey);

        /// <summary>
        ///     Loads the <see cref="StatementModel" />.
        /// </summary>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <exception cref="System.ArgumentNullException">Will be thrown if any arguments are null.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        ///     Will be thrown if the specified storage key is not
        ///     found.
        /// </exception>
        /// <exception cref="System.NotSupportedException">The CSV file is not supported by this version of the Budget Analyser.</exception>
        /// <exception cref="StatementModelChecksumException">
        ///     Will be thrown if the checksum doesn't match the data within the
        ///     file.
        /// </exception>
        /// <exception cref="DataFormatException">Will be thrown if the file format is invalid.</exception>
        Task<StatementModel> LoadAsync([NotNull] string storageKey, bool isEncrypted);

        /// <summary>
        ///     Saves the <see cref="StatementModel" />.
        /// </summary>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <param name="model">The statement model to save.</param>
        /// <param name="storageKey">The unique storage identifier</param>
        /// <exception cref="System.ArgumentNullException">Will be thrown if any arguments are null.</exception>
        /// <exception cref="BudgetAnalyser.Engine.Statement.StatementModelChecksumException">
        ///     Will be thrown if serialisation
        ///     resulted in data that doesn't match the checksum.
        /// </exception>
        Task SaveAsync([NotNull] StatementModel model, [NotNull] string storageKey, bool isEncrypted);
    }
}