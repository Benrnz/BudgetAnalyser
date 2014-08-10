using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskBudgetRepository : IBudgetRepository, IApplicationHookEventPublisher
    {
        private readonly BasicMapper<BudgetCollection, BudgetCollectionDto> toDtoMapper;
        private readonly BasicMapper<BudgetCollectionDto, BudgetCollection> toDomainMapper;

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

            BudgetBucketRepository = bucketRepository;
            this.toDtoMapper = toDtoMapper;
            this.toDomainMapper = toDomainMapper;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }

        public BudgetCollection CreateNew([NotNull] string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            var newBudget = new BudgetModel
            {
                EffectiveFrom = DateTime.Today,
                Name = "Default Budget",
            };

            var newCollection = new BudgetCollection(new[] { newBudget })
            {
                FileName = fileName
            };

            BudgetBucketRepository.Initialise(new List<BudgetBucketDto>());

            Save(newCollection);

            return newCollection;
        }

        public BudgetCollection Load(string fileName)
        {
            if (!FileExists(fileName))
            {
                throw new FileNotFoundException("File not found.", fileName);
            }

            object serialised;
            try
            {
                serialised = LoadFromDisk(fileName); // May return null for some errors.
            }
            catch (XamlObjectWriterException ex)
            {
                throw new FileFormatException(
                    string.Format(CultureInfo.CurrentCulture, "The budget file '{0}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", fileName), ex);
            }
            catch (Exception ex)
            {
                throw new FileFormatException("Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            var correctDataFormat = serialised as BudgetCollectionDto;
            if (correctDataFormat == null)
            {
                throw new FileFormatException(
                    string.Format(CultureInfo.InvariantCulture, "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.", fileName));
            }

            // Bucket Repository must be initialised first, the budget model incomes/expenses are dependent on the bucket repository.
            BudgetBucketRepository.Initialise(correctDataFormat.Buckets);

            BudgetCollection budgetCollection = this.toDomainMapper.Map(correctDataFormat);
            budgetCollection.FileName = fileName;
            return budgetCollection;
        }

        public void Save(BudgetCollection budget)
        {
            BudgetCollectionDto dataFormat = this.toDtoMapper.Map(budget);

            string serialised = Serialise(dataFormat);
            WriteToDisk(dataFormat.FileName, serialised);

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "BudgetRepository", ApplicationHookEventArgs.Save));
            }
        }

        protected virtual bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        protected virtual object LoadFromDisk(string fileName)
        {
            return XamlServices.Load(fileName);
        }

        protected virtual string Serialise(BudgetCollectionDto budgetData)
        {
            return XamlServices.Save(budgetData);
        }

        protected virtual void WriteToDisk(string fileName, string data)
        {
            File.WriteAllText(fileName, data);
        }
    }
}