using System.Globalization;
using System.Text;
using Portable.Xaml;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Ledger
{
    /// <summary>
    ///     A repository for Ledger Books that persists to local disk in Xaml format.
    /// </summary>
    /// <seealso cref="ILedgerBookRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class XamlOnDiskLedgerBookRepository : ILedgerBookRepository
    {
        private readonly BankImportUtilities importUtilities;
        private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper;
        private readonly IReaderWriterSelector readerWriterSelector;

        public XamlOnDiskLedgerBookRepository(
            [NotNull] IDtoMapper<LedgerBookDto, LedgerBook> mapper,
            [NotNull] BankImportUtilities importUtilities,
            [NotNull] IReaderWriterSelector readerWriterSelector)
        {
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            this.importUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
            this.readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));
        }

        public async Task<LedgerBook> CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var book = new LedgerBook
            {
                Name = Path.GetFileNameWithoutExtension(storageKey).Replace('.', ' '),
                StorageKey = storageKey,
                Modified = DateTime.Now
            };

            await SaveAsync(book, storageKey, false);
            return await LoadAsync(storageKey, false);
        }

        public async Task<LedgerBook> LoadAsync(string storageKey, bool isEncrypted)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            LedgerBookDto dataEntity;
            try
            {
                this.importUtilities.AbortIfFileDoesntExist(storageKey);
                dataEntity = await LoadXamlFromDiskAsync(storageKey, isEncrypted);
            }
            catch (FileNotFoundException ex)
            {
                throw new KeyNotFoundException("Data file can not be found: " + storageKey, ex);
            }
            catch (Exception ex)
            {
                throw new DataFormatException(
                    "Deserialisation Ledger Book file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.",
                    ex);
            }

            if (dataEntity is null)
            {
                throw new DataFormatException(string.Format(CultureInfo.CurrentCulture,
                    "The specified file {0} is not of type Data-Ledger-Book", storageKey));
            }

            dataEntity.StorageKey = storageKey;
            var book = this.mapper.ToModel(dataEntity);

            var messages = new StringBuilder();
            if (!book.Validate(messages))
            {
                throw new DataFormatException(messages.ToString());
            }

            if (Math.Abs(dataEntity.Checksum - -1) < 0.0001)
            {
                // bypass checksum check - this is to allow intentional manual changes to the file.  This checksum is only trying to prevent
                // bugs in code from breaking the consistency of the file.
            }
            else
            {
                var calculatedChecksum = CalculateChecksum(book);
                if (Math.Abs(calculatedChecksum - dataEntity.Checksum) > 0.0001)
                {
                    throw new DataFormatException("The Ledger Book has been tampered with, checksum should be " +
                                                  calculatedChecksum);
                }
            }

            return book;
        }

        public async Task SaveAsync(LedgerBook book, string storageKey, bool isEncrypted)
        {
            if (book is null)
            {
                throw new ArgumentNullException(nameof(book));
            }

            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var dataEntity = this.mapper.ToDto(book);
            book.StorageKey = storageKey;
            dataEntity.StorageKey = storageKey;
            dataEntity.Checksum = CalculateChecksum(book);

            await SaveDtoToDiskAsync(dataEntity, isEncrypted);
        }

        protected virtual object Deserialise(string xaml)
        {
            return XamlServices.Parse(xaml);
        }

        /// <summary>
        ///     Loads the xaml as string.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        protected virtual async Task<LedgerBookDto> LoadXamlFromDiskAsync(string fileName, bool isEncrypted)
        {
            var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
            var result = await reader.LoadFromDiskAsync(fileName);
            return Deserialise(result) as LedgerBookDto;
        }

        protected virtual async Task SaveDtoToDiskAsync([NotNull] LedgerBookDto dataEntity, bool isEncrypted)
        {
            if (dataEntity is null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
            await writer.WriteToDiskAsync(dataEntity.StorageKey, Serialise(dataEntity));
        }

        /// <summary>
        ///     Serialises the specified data entity.
        /// </summary>
        /// <param name="dataEntity">The data entity.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected virtual string Serialise(LedgerBookDto dataEntity)
        {
            if (dataEntity is null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            return XamlServices.Save(dataEntity);
        }

        private static double CalculateChecksum(LedgerBook dataEntity)
        {
            // ReSharper disable once EnumerableSumInExplicitUncheckedContext - Used to calculate a checksum and revolving (overflowing) integers are ok here.
            return dataEntity.Reconciliations.Sum(
                l =>
                    (double) l.LedgerBalance
                    + l.BankBalanceAdjustments.Sum(b => (double) b.Amount)
                    + l.Entries.Sum(e => (double) e.Balance));
        }
    }
}