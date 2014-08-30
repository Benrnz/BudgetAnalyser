using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    public class BankImportUtilities
    {
        private readonly ILogger logger;

        public BankImportUtilities([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        internal virtual void AbortIfFileDoesntExist(string fileName, IUserMessageBox messageBox)
        {
            if (!File.Exists(fileName))
            {
                messageBox.Show("The file name provided no longer exists at its location.", "File Not Found");
                throw new FileNotFoundException("File not found.", fileName);
            }
        }

        internal BudgetBucket FetchBudgetBucket([NotNull] string[] array, int index, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            string stringType = SafeArrayFetchString(array, index);
            if (string.IsNullOrWhiteSpace(stringType))
            {
                return null;
            }

            stringType = stringType.ToUpperInvariant();

            return bucketRepository.GetByCode(stringType);
        }

        internal DateTime SafeArrayFetchDate([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            string stringToParse = array[index];
            DateTime retval;
            if (!DateTime.TryParse(stringToParse, out retval))
            {
                this.logger.LogWarning(l => "BankImportUtilities: Unable to parse date: " + stringToParse);
                return DateTime.MinValue;
            }

            return retval;
        }

        internal Decimal SafeArrayFetchDecimal([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            string stringToParse = array[index];
            Decimal retval;
            if (!Decimal.TryParse(stringToParse, out retval))
            {
                this.logger.LogWarning(l => "BankImportUtilities: Unable to parse decimal: " + stringToParse);
                return 0;
            }

            return retval;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Preferable with IoC")]
        internal Guid SafeArrayFetchGuid([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            string stringToParse = array[index];
            Guid result;
            if (!Guid.TryParse(stringToParse, out result))
            {
                result = Guid.NewGuid();
            }

            return result;
        }

        internal long SafeArrayFetchLong([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            string stringToParse = array[index];
            long retval;
            if (!long.TryParse(stringToParse, out retval))
            {
                this.logger.LogWarning(l => "BankImportUtilities: Unable to parse long: " + stringToParse);
                return 0;
            }

            return retval;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Preferable with IoC")]
        internal string SafeArrayFetchString([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            return array[index].Trim();
        }

        private static void ThrowIndexOutOfRangeException(string[] array, int index)
        {
            throw new UnexpectedIndexException(string.Format(CultureInfo.CurrentCulture, "Index {0} is out of range for array with length {1}.", index, array.Length));
        }
    }
}