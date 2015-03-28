using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskBudgetRepository : IBudgetRepository
    {
        private readonly IBudgetBucketRepository budgetBucketRepository;
        private readonly BasicMapper<BudgetCollectionDto, BudgetCollection> toDomainMapper;
        private readonly BasicMapper<BudgetCollection, BudgetCollectionDto> toDtoMapper;
        private BudgetCollection currentBudgetCollection;

        public XamlOnDiskBudgetRepository(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] BasicMapper<BudgetCollection, BudgetCollectionDto> toDtoMapper,
            [NotNull] BasicMapper<BudgetCollectionDto, BudgetCollection> toDomainMapper)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (toDtoMapper == null)
            {
                throw new ArgumentNullException("toDtoMapper");
            }

            if (toDomainMapper == null)
            {
                throw new ArgumentNullException("toDomainMapper");
            }

            this.budgetBucketRepository = bucketRepository;
            this.toDtoMapper = toDtoMapper;
            this.toDomainMapper = toDomainMapper;
        }

        public BudgetCollection CreateNew()
        {
            var budget = new BudgetModel();
            this.currentBudgetCollection = new BudgetCollection(new[] { budget });
            this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());
            return this.currentBudgetCollection;
        }

        public async Task<BudgetCollection> CreateNewAsync(string fileName)
        {
            if (fileName.IsNothing())
            {
                throw new ArgumentNullException("fileName");
            }

            var newBudget = new BudgetModel
            {
                EffectiveFrom = DateTime.Today,
                Name = Path.GetFileNameWithoutExtension(fileName).Replace('.', ' ')
            };

            this.currentBudgetCollection = new BudgetCollection(new[] { newBudget })
            {
                FileName = fileName
            };

            this.budgetBucketRepository.Initialise(new List<BudgetBucketDto>());

            await SaveAsync();
            return this.currentBudgetCollection;
        }

        public async Task<BudgetCollection> LoadAsync(string fileName)
        {
            if (fileName.IsNothing())
            {
                throw new ArgumentNullException("fileName");
            }

            if (!FileExists(fileName))
            {
                throw new KeyNotFoundException("File not found. " + fileName);
            }

            object serialised;
            try
            {
                serialised = await LoadFromDisk(fileName); // May return null for some errors.
            }
            catch (XamlObjectWriterException ex)
            {
                throw new DataFormatException(
                    string.Format(CultureInfo.CurrentCulture, "The budget file '{0}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", fileName),
                    ex);
            }
            catch (Exception ex)
            {
                throw new DataFormatException("Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            var correctDataFormat = serialised as BudgetCollectionDto;
            if (correctDataFormat == null)
            {
                throw new DataFormatException(
                    string.Format(CultureInfo.InvariantCulture, "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.", fileName));
            }

            // Bucket Repository must be initialised first, the budget model incomes/expenses are dependent on the bucket repository.
            this.budgetBucketRepository.Initialise(correctDataFormat.Buckets);

            BudgetCollection budgetCollection = this.toDomainMapper.Map(correctDataFormat);
            budgetCollection.FileName = fileName;
            this.currentBudgetCollection = budgetCollection;
            return budgetCollection;
        }

        public async Task SaveAsync()
        {
            if (this.currentBudgetCollection == null)
            {
                throw new InvalidOperationException("There is no current budget collection loaded.");
            }

            BudgetCollectionDto dataFormat = this.toDtoMapper.Map(this.currentBudgetCollection);

            string serialised = Serialise(dataFormat);
            await WriteToDisk(dataFormat.FileName, serialised);
        }

        protected virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        protected virtual async Task<object> LoadFromDisk(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Load(fileName));
            return result;
        }

        protected virtual string Serialise(BudgetCollectionDto budgetData)
        {
            return XamlServices.Save(budgetData);
        }

        protected virtual async Task WriteToDisk(string fileName, string data)
        {
            using (var file = new StreamWriter(fileName, false))
            {
                await file.WriteAsync(data);
            }
        }
    }
}