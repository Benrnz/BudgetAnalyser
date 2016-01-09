using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget.Data;
using JetBrains.Annotations;
using Portable.Xaml;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Budget
{
    /// <summary>
    ///     A repository to store the budget collections on local disk as a Xaml file.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Budget.IBudgetRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskBudgetRepository : IBudgetRepository
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper;
        private BudgetCollection currentBudgetCollection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskBudgetRepository" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public XamlOnDiskBudgetRepository(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            this.budgetBucketRepository = bucketRepository;
            this.mapper = mapper;
        }

        /// <summary>
        ///     Creates a new empty <see cref="BudgetCollection" /> but does not save it.
        /// </summary>
        public BudgetCollection CreateNew()
        {
            var budget = new BudgetModel();
            this.currentBudgetCollection = new BudgetCollection(budget);
            this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());
            return this.currentBudgetCollection;
        }

        /// <summary>
        ///     Creates a new empty <see cref="BudgetCollection" /> at the location indicated by the <see paramref="storageKey" />.
        ///     Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to load the new collection.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<BudgetCollection> CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var newBudget = new BudgetModel
            {
                EffectiveFrom = DateTime.Today,
                Name = Path.GetFileNameWithoutExtension(storageKey).Replace('.', ' ')
            };

            this.currentBudgetCollection = new BudgetCollection(newBudget)
            {
                StorageKey = storageKey
            };

            this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());

            await SaveAsync();
            return this.currentBudgetCollection;
        }

        /// <summary>
        ///     Loads the a <see cref="BudgetCollection" /> from storage at the location indicated by <see paramref="storageKey" />
        ///     .
        /// </summary>
        /// <param name="storageKey"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException">File not found.  + storageKey</exception>
        /// <exception cref="DataFormatException">
        ///     Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is
        ///     invalid.
        ///     or
        /// </exception>
        public async Task<BudgetCollection> LoadAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            if (!FileExists(storageKey))
            {
                throw new KeyNotFoundException("File not found. " + storageKey);
            }

            object serialised;
            try
            {
                serialised = await LoadFromDisk(storageKey); // May return null for some errors.
            }
            catch (XamlObjectWriterException ex)
            {
                throw new DataFormatException(
                    string.Format(CultureInfo.CurrentCulture,
                        "The budget file '{0}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.",
                        storageKey),
                    ex);
            }
            catch (Exception ex)
            {
                throw new DataFormatException(
                    "Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.",
                    ex);
            }

            var correctDataFormat = serialised as BudgetCollectionDto;
            if (correctDataFormat == null)
            {
                throw new DataFormatException(
                    string.Format(CultureInfo.InvariantCulture,
                        "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.",
                        storageKey));
            }

            // Bucket Repository must be initialised first, the budget model incomes/expenses are dependent on the bucket repository.
            this.budgetBucketRepository.Initialise(correctDataFormat.Buckets);

            BudgetCollection budgetCollection = this.mapper.ToModel(correctDataFormat);
            budgetCollection.StorageKey = storageKey;
            this.currentBudgetCollection = budgetCollection;
            return budgetCollection;
        }

        /// <summary>
        ///     Saves the current <see cref="BudgetCollection" /> to storage.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no current budget collection loaded.</exception>
        public async Task SaveAsync()
        {
            if (this.currentBudgetCollection == null)
            {
                throw new InvalidOperationException("There is no current budget collection loaded.");
            }

            BudgetCollectionDto dataFormat = this.mapper.ToDto(this.currentBudgetCollection);

            var serialised = Serialise(dataFormat);
            await WriteToDisk(dataFormat.StorageKey, serialised);
        }

        /// <summary>
        ///     Files the exists.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        protected virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        /// <summary>
        ///     Loads a budget collection xaml file from disk.
        /// </summary>
        /// <param name="fileName">Full path and filename of the file.</param>
        protected virtual async Task<object> LoadFromDisk(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Load(fileName));
            return result;
        }

        /// <summary>
        ///     Serialises the specified budget data.
        /// </summary>
        protected virtual string Serialise(BudgetCollectionDto budgetData)
        {
            return XamlServices.Save(budgetData);
        }

        /// <summary>
        ///     Writes the budget collections to a xaml file on disk.
        /// </summary>
        protected virtual async Task WriteToDisk(string fileName, string data)
        {
            using (var fileStream = new StreamWriter(new FileStream(fileName, FileMode.Create)))
            {
                await fileStream.WriteAsync(data);
            }
        }
    }
}