using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Matching.Data;
using JetBrains.Annotations;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A Repository to persistently store matching rules in Xaml format on local disk.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Matching.IMatchingRuleRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskMatchingRuleRepository : IMatchingRuleRepository
    {
        private readonly ILogger logger;
        private readonly IDtoMapper<MatchingRuleDto, MatchingRule> mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskMatchingRuleRepository" /> class.
        /// </summary>
        /// <param name="mapper">The data to domain mapper.</param>
        /// <param name="logger">The diagnostics logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public XamlOnDiskMatchingRuleRepository([NotNull] IDtoMapper<MatchingRuleDto, MatchingRule> mapper, [NotNull] ILogger logger)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.mapper = mapper;
            this.logger = logger;
        }

        /// <summary>
        ///     Creates a new empty collection of <see cref="MatchingRule" />. The new collection is not saved.
        /// </summary>
        public IEnumerable<MatchingRule> CreateNew()
        {
            return new List<MatchingRule>();
        }

        /// <summary>
        ///     Creates a new empty collection of <see cref="MatchingRule" />s at the location indicated by the
        ///     <paramref name="storageKey" />. Any existing data at this location will be overwritten. After this is complete, use
        ///     the <see cref="LoadAsync" /> method to load the new collection.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            await SaveAsync(new List<MatchingRule>(), storageKey);
        }

        /// <summary>
        ///     Loads the rules collection from persistent storage.
        /// </summary>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">
        ///     storageKey is blank
        ///     or
        ///     Storage key can not be found:  + storageKey
        /// </exception>
        /// <exception cref="DataFormatException">
        ///     Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is
        ///     invalid or Deserialised Matching-Rules are not of type List{MatchingRuleDto}
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MatchingRuleDto")]
        public async Task<IEnumerable<MatchingRule>> LoadAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new KeyNotFoundException("storageKey is blank");
            }

            if (!Exists(storageKey))
            {
                throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
            }

            List<MatchingRuleDto> dataEntities;
            try
            {
                dataEntities = await LoadFromDiskAsync(storageKey);
            }
            catch (Exception ex)
            {
                throw new DataFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            if (dataEntities == null)
            {
                throw new DataFormatException("Deserialised Matching-Rules are not of type List<MatchingRuleDto>");
            }

            IEnumerable<MatchingRule> realModel = dataEntities.Select(d => this.mapper.ToModel(d));
            return Validate(realModel.ToList());
        }

        /// <summary>
        ///     Saves the rules collection to persistent storage.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public async Task SaveAsync(IEnumerable<MatchingRule> rules, string storageKey)
        {
            if (rules == null)
            {
                throw new ArgumentNullException(nameof(rules));
            }

            if (storageKey == null)
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            IEnumerable<MatchingRule> model = Validate(rules.ToList());
            IEnumerable<MatchingRuleDto> dataEntities = model.Select(r => this.mapper.ToDto(r));
            await SaveToDiskAsync(storageKey, dataEntities);
        }

        /// <summary>
        ///     Returns true if the matching rule collection identified by the given storage key exists or not.
        /// </summary>
        protected virtual bool Exists(string storageKey)
        {
            return File.Exists(storageKey);
        }

        /// <summary>
        ///     Loads the rules collection from local disk.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists",
            Justification = "Necessary for persistence - this is the type of the rehydrated object")]
        protected virtual async Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlFromDisk(fileName)));
            return result as List<MatchingRuleDto>;
        }

        /// <summary>
        ///     Loads the xaml from disk.
        /// </summary>
        protected virtual string LoadXamlFromDisk(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        /// <summary>
        ///     Saves the data to disk.
        /// </summary>
        protected virtual async Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities)
        {
            await Task.Run(() =>
            {
                using (var stream = new FileStream(fileName, FileMode.Create))
                {
                    XamlServices.Save(stream, dataEntities.ToList());
                }
            });
        }

        private IList<MatchingRule> Validate(IList<MatchingRule> model)
        {
            // Remove duplicates.
            var duplicatesExist = model.GroupBy(r => r.RuleId).Any(g => g.Count() > 1);
            if (!duplicatesExist)
            {
                return model;
            }

            var knownList = new HashSet<Guid>();
            var indexOfDuplicate = 0;
            bool foundDuplicate;
            do
            {
                foundDuplicate = false;
                for (var index = indexOfDuplicate; index < model.Count; index++)
                {
                    if (knownList.Contains(model[index].RuleId))
                    {
                        indexOfDuplicate = index;
                        foundDuplicate = true;
                        break;
                    }
                    knownList.Add(model[index].RuleId);
                }

                if (foundDuplicate)
                {
                    MatchingRule rule = model[indexOfDuplicate];
                    this.logger.LogWarning(l =>
                            $"Duplicate RuleID found and will be removed: {rule.RuleId} {rule.BucketCode} {rule.LastMatch:o} And:{rule.And} {rule.Description} {rule.TransactionType} {rule.Reference1}");
                    model.RemoveAt(indexOfDuplicate);
                }
            } while (foundDuplicate);

            return model;
        }
    }
}