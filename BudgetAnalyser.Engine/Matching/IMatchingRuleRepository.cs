using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

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
        ///     <see cref="storageKey" />. Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to
        ///     load the new collection.
        /// </summary>
        Task CreateNewAndSaveAsync([NotNull] string storageKey);

        Task<IEnumerable<MatchingRule>> LoadAsync([NotNull] string storageKey);
        Task SaveAsync([NotNull] IEnumerable<MatchingRule> rules, [NotNull] string storageKey);
    }
}