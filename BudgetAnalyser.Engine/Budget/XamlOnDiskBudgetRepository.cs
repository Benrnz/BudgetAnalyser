using System;
using System.Globalization;
using System.IO;
using System.Net.Mime;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskBudgetRepository : IBudgetRepository, IApplicationHookEventPublisher
    {
        public XamlOnDiskBudgetRepository([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            BudgetBucketRepository = bucketRepository;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }

        public BudgetCollection CreateNew(string fileName)
        {
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
                throw new FileFormatException("The budget file '{0}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", ex);
            }
            catch (Exception ex)
            {
                throw new FileFormatException("Deserialisation the Budget file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            var correctFormat = serialised as BudgetCollection;
            if (correctFormat == null)
            {
                throw new FileFormatException(
                    string.Format(CultureInfo.InvariantCulture, "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.", fileName));
            }

            correctFormat.FileName = fileName;
            BudgetBucketRepository.Initialise(correctFormat);
            correctFormat.Initialise();
            return correctFormat;
        }

        public void Save(BudgetCollection budgetData)
        {
            string serialised = Serialise(budgetData);
            WriteToDisk(budgetData.FileName, serialised);

            var handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "BudgetRepository"));
            }
        }

        protected virtual bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        protected virtual object LoadFromDisk(string filename)
        {
            return XamlServices.Load(filename);
        }

        protected virtual string Serialise(BudgetCollection budgetData)
        {
            return XamlServices.Save(budgetData);
        }

        protected virtual void WriteToDisk(string filename, string data)
        {
            File.WriteAllText(filename, data);
        }
    }
}