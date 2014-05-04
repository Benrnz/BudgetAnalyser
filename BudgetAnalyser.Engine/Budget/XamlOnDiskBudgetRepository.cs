using System;
using System.Globalization;
using System.IO;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskBudgetRepository : IBudgetRepository, IApplicationHookEventPublisher
    {
        private readonly BudgetCollectionToDataBudgetCollectionMapper toDataMapper;
        private readonly DataBudgetCollectionToBudgetCollectionMapper toDomainMapper;

        public XamlOnDiskBudgetRepository(
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] BudgetCollectionToDataBudgetCollectionMapper toDataMapper,
            [NotNull] DataBudgetCollectionToBudgetCollectionMapper toDomainMapper)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (toDataMapper == null)
            {
                throw new ArgumentNullException("toDataMapper");
            }

            if (toDomainMapper == null)
            {
                throw new ArgumentNullException("toDomainMapper");
            }

            BudgetBucketRepository = bucketRepository;
            this.toDataMapper = toDataMapper;
            this.toDomainMapper = toDomainMapper;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom collection")]
        public BudgetCollection CreateNew([NotNull] string fileName)
        {
            if (fileName == null)
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

            Save(newCollection);

            return newCollection;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom collection")]
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
                throw new FileFormatException(string.Format(CultureInfo.CurrentCulture, "The budget file '{0}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", fileName), ex);
            }
            catch (Exception ex)
            {
                throw new FileFormatException("Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            var correctDataFormat = serialised as DataBudgetCollection;
            if (correctDataFormat == null)
            {
                throw new FileFormatException(
                    string.Format(CultureInfo.InvariantCulture, "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.", fileName));
            }

            var correctFormat = this.toDomainMapper.Map(correctDataFormat);
            correctFormat.FileName = fileName;
            BudgetBucketRepository.Initialise(correctFormat);
            return correctFormat;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification="Custom collection")]
        public void Save(BudgetCollection budget)
        {
            var dataFormat = this.toDataMapper.Map(budget);
            string serialised = Serialise(dataFormat);
            WriteToDisk(dataFormat.FileName, serialised);

            var handler = ApplicationEvent;
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

        protected virtual string Serialise(DataBudgetCollection budgetData)
        {
            return XamlServices.Save(budgetData);
        }

        protected virtual void WriteToDisk(string fileName, string data)
        {
            File.WriteAllText(fileName, data);
        }
    }
}