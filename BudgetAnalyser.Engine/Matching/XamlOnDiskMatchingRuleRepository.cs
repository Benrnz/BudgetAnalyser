using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Matching.Data;
using JetBrains.Annotations;
using Portable.Xaml;

namespace BudgetAnalyser.Engine.Matching
{
    /// <summary>
    ///     A Repository to persistently store matching rules in Xaml format on local disk.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Matching.IMatchingRuleRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskMatchingRuleRepository : IMatchingRuleRepository
    {
        private readonly IDtoMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskMatchingRuleRepository" /> class.
        /// </summary>
        /// <param name="dataToDomainMapper">The data to domain mapper.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public XamlOnDiskMatchingRuleRepository([NotNull] IDtoMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper)
        {
            if (dataToDomainMapper == null)
            {
                throw new ArgumentNullException(nameof(dataToDomainMapper));
            }

            this.dataToDomainMapper = dataToDomainMapper;
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
        ///     invalid.
        ///     or
        ///     Deserialised Matching-Rules are not of type List{MatchingRuleDto}
        /// </exception>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly",
            MessageId = "MatchingRuleDto")]
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
                throw new DataFormatException(
                    "Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.",
                    ex);
            }

            if (dataEntities == null)
            {
                throw new DataFormatException("Deserialised Matching-Rules are not of type List<MatchingRuleDto>");
            }

            return dataEntities.Select(d => this.dataToDomainMapper.ToModel(d));
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

            var dataEntities = rules.Select(r => this.dataToDomainMapper.ToDto(r));
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
    }
}