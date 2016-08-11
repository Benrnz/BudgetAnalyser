using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Services;
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
    internal class XamlOnDiskBudgetRepository : IBudgetRepository
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper;
        private readonly IReaderWriterSelector readerWriterSelector;
        private BudgetCollection currentBudgetCollection;
        private bool isEncryptedAtLastAccess;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskBudgetRepository" /> class.
        /// </summary>
        public XamlOnDiskBudgetRepository(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper,
            [NotNull] IReaderWriterSelector readerWriterSelector)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (readerWriterSelector == null) throw new ArgumentNullException(nameof(readerWriterSelector));

            this.budgetBucketRepository = bucketRepository;
            this.mapper = mapper;
            this.readerWriterSelector = readerWriterSelector;
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

            await SaveAsync(storageKey, false);
            return this.currentBudgetCollection;
        }

        public async Task<BudgetCollection> LoadAsync(string storageKey, bool isEncrypted)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            this.isEncryptedAtLastAccess = isEncrypted;
            var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);

            if (!reader.FileExists(storageKey))
            {
                throw new KeyNotFoundException("File not found. " + storageKey);
            }

            object serialised;
            try
            {
                var xaml = await reader.LoadFromDiskAsync(storageKey); // May return null for some errors.
                serialised = Deserialise(xaml);
            }
            catch (XamlObjectWriterException ex)
            {
                throw new DataFormatException($"The budget file '{storageKey}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", ex);
            }
            catch (EncryptionKeyIncorrectException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataFormatException("Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
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

            var budgetCollection = this.mapper.ToModel(correctDataFormat);
            budgetCollection.StorageKey = storageKey;
            this.currentBudgetCollection = budgetCollection;
            return budgetCollection;
        }

        public async Task SaveAsync()
        {
            if (this.currentBudgetCollection == null)
            {
                throw new InvalidOperationException("There is no current budget collection loaded.");
            }

            await SaveAsync(this.currentBudgetCollection.StorageKey, this.isEncryptedAtLastAccess);
        }

        public async Task SaveAsync(string storageKey, bool isEncrypted)
        {
            if (this.currentBudgetCollection == null)
            {
                throw new InvalidOperationException("There is no current budget collection loaded.");
            }

            this.isEncryptedAtLastAccess = isEncrypted;
            var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);

            this.currentBudgetCollection.StorageKey = storageKey;
            var dataFormat = this.mapper.ToDto(this.currentBudgetCollection);
            var serialised = Serialise(dataFormat);
            await writer.WriteToDiskAsync(dataFormat.StorageKey, serialised);
        }

        protected virtual object Deserialise(string xaml)
        {
            return XamlServices.Parse(xaml);
        }

        /// <summary>
        ///     Serialises the specified budget data.
        /// </summary>
        protected virtual string Serialise(BudgetCollectionDto budgetData)
        {
            return XamlServices.Save(budgetData);
        }
    }
}