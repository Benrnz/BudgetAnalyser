using System;
using System.Globalization;
using System.IO;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetRepository : IBudgetRepository
    {
        public BudgetRepository([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            BudgetBucketRepository = bucketRepository;
        }

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
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found.", fileName);
            }

            object serialised;
            try
            {
                serialised = XamlServices.Load(fileName); // Will always succeed without exceptions even if bad file format, but will return null.
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
            string serialised = XamlServices.Save(budgetData);
            File.WriteAllText(budgetData.FileName, serialised);
        }
    }
}