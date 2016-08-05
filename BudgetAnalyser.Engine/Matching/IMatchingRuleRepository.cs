using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     An interface to describe persistence functions to retrieve and save a collection of <see cref="MatchingRule" />s.
    /// </summary>
    public interface IMatchingRuleRepository
    {
        /// <summary>
        ///     Creates a new empty collection of <see cref="MatchingRule" />. The new collection is not saved.
        /// </summary>
        IEnumerable<MatchingRule> CreateNew();

        /// <summary>
        ///     Creates a new empty collection of <see cref="MatchingRule" />s at the location indicated by the
        ///     <paramref name="storageKey" />. Any existing data at this location will be overwritten. After this is complete, use
        ///     the <see cref="LoadAsync" /> method to load the new collection.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Will be thrown if the storage key is null.</exception>
        Task CreateNewAndSaveAsync([NotNull] string storageKey);

        /// <summary>
        ///     Loads the rules collection from persistent storage.
        /// </summary>
        /// <param name="storageKey">The unique storage identifier.</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        ///     Will be thrown when the storageKey is blank
        ///     or
        ///     Storage key can not be found.
        /// </exception>
        /// <exception cref="DataFormatException">
        ///     Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is
        ///     invalid or Deserialised Matching-Rules are not of type List{MatchingRuleDto}
        /// </exception>
        /// <exception cref="System.ArgumentNullException">Will be thrown if the storage key is null.</exception>
        Task<IEnumerable<MatchingRule>> LoadAsync([NotNull] string storageKey, bool isEncrypted);

        /// <summary>
        ///     Saves the rules collection to persistent storage.
        /// </summary>
        /// <param name="rules">The rules collection to save.</param>
        /// <param name="storageKey">The unique storage identifier.</param>
        /// <param name="isEncrypted">A boolean to indicate if the data file should be encrypted or not.</param>
        /// <exception cref="System.ArgumentNullException">Will be thrown if any arguments are null.</exception>
        Task SaveAsync([NotNull] IEnumerable<MatchingRule> rules, [NotNull] string storageKey, bool isEncrypted);
    }
}