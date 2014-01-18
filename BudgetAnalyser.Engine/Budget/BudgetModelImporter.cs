using System;
using System.Globalization;
using System.IO;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Budget
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetModelImporter : IBudgetModelImporter
    {
        public BudgetModelImporter([NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            this.BudgetBucketRepository = bucketRepository;
        }

        public BudgetCollection LoadBudgetData(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found.", fileName);
            }

            object serialised = null;
            try
            {
                serialised = XamlServices.Load(fileName); // Will always succeed without exceptions even if bad file format, but will return null.
            }
            catch (XamlObjectWriterException ex)
            {
                throw new FileFormatException("The budget file '{0}' is an invalid format. This is probably due to changes in the code, most likely namespace changes.", ex);
            } 

            var correctFormat = serialised as BudgetCollection;
            if (correctFormat == null)
            {
                throw new FileFormatException(
                    string.Format(CultureInfo.InvariantCulture, "The file used to store application state ({0}) is not in the correct format. It may have been tampered with.", fileName));
            }

            correctFormat.FileName = fileName;
            this.BudgetBucketRepository.Initialise(correctFormat);
            return correctFormat;
        }

        public void SaveBudgetData(BudgetCollection budgetData)
        {
            string serialised = XamlServices.Save(budgetData);
            File.WriteAllText(budgetData.FileName, serialised);
        }

        public IBudgetBucketRepository BudgetBucketRepository { get; private set; }
    }
}