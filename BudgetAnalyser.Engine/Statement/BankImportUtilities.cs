using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using BudgetAnalyser.Engine.Budget;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A set of utilities used when importing bank data
    /// </summary>
    [AutoRegisterWithIoC]
    internal class BankImportUtilities
    {
        private readonly ILogger logger;
        private CultureInfo locale;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BankImportUtilities" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public BankImportUtilities([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.logger = logger;
            this.locale = CultureInfo.CurrentCulture;
        }

        internal virtual void AbortIfFileDoesntExist(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException("File not found.", fileName);
            }
        }

        internal void ConfigureLocale(CultureInfo culture)
        {
            this.locale = culture;
        }

        internal BudgetBucket FetchBudgetBucket([NotNull] string[] array, int index, [NotNull] IBudgetBucketRepository bucketRepository)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException(nameof(bucketRepository));
            }

            var stringType = FetchString(array, index);
            if (string.IsNullOrWhiteSpace(stringType))
            {
                return null;
            }

            stringType = stringType.ToUpperInvariant();

            return bucketRepository.GetByCode(stringType);
        }

        internal DateTime FetchDate([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            var stringToParse = array[index];
            DateTime retval;
            if (!DateTime.TryParse(stringToParse, this.locale, DateTimeStyles.None, out retval))
            {
                this.logger.LogError(l => "BankImportUtilities: Unable to parse date: " + stringToParse);
                throw new InvalidDataException("Expected date, but provided data is invalid. " + stringToParse);
            }

            return retval;
        }

        internal decimal FetchDecimal([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            var stringToParse = array[index];
            decimal retval;
            if (!decimal.TryParse(stringToParse, out retval))
            {
                this.logger.LogError(l => "BankImportUtilities: Unable to parse decimal: " + stringToParse);
                throw new InvalidDataException("Expected decimal, but provided data is invalid. " + stringToParse);
            }

            return retval;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Preferable with IoC")]
        internal Guid FetchGuid([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            var stringToParse = array[index];
            Guid result;
            if (!Guid.TryParse(stringToParse, out result))
            {
                this.logger.LogError(l => "BankImportUtilities: Unable to parse Guid: " + stringToParse);
                throw new InvalidDataException("Expected Guid, but provided data is invalid. " + stringToParse);
            }

            return result;
        }

        internal long FetchLong([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index > array.Length - 1 || index < 0)
            {
                ThrowIndexOutOfRangeException(array, index);
            }

            var stringToParse = array[index];
            long retval;
            if (!long.TryParse(stringToParse, out retval))
            {
                this.logger.LogError(l => "BankImportUtilities: Unable to parse long: " + stringToParse);
                throw new InvalidDataException("Expected long, but provided data is invalid. " + stringToParse);
            }

            return retval;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Preferable with IoC")]
        internal string FetchString([NotNull] string[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
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