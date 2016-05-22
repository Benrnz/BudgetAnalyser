using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Statement;
using JetBrains.Annotations;
using Portable.Xaml;
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
        private readonly ILedgerBookFactory ledgerBookFactory;
        private readonly IDtoMapper<LedgerBookDto, LedgerBook> mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="XamlOnDiskLedgerBookRepository" /> class.
        /// </summary>
        /// <param name="mapper">The data to domain mapper.</param>
        /// <param name="importUtilities">The import utilities.</param>
        /// <param name="ledgerBookFactory">The ledger book factory.</param>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public XamlOnDiskLedgerBookRepository(
            [NotNull] IDtoMapper<LedgerBookDto, LedgerBook> mapper,
            [NotNull] BankImportUtilities importUtilities,
            [NotNull] ILedgerBookFactory ledgerBookFactory)
        {
            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }

            if (importUtilities == null)
            {
                throw new ArgumentNullException(nameof(importUtilities));
            }

            if (ledgerBookFactory == null)
            {
                throw new ArgumentNullException(nameof(ledgerBookFactory));
            }

            this.mapper = mapper;
            this.importUtilities = importUtilities;
            this.ledgerBookFactory = ledgerBookFactory;
        }

        /// <summary>
        ///     Creates a new empty <see cref="LedgerBook" /> at the location indicated by the <paramref name="storageKey" />. Any
        ///     existing data at this location will be overwritten. After this is complete, use the <see cref="LoadAsync" /> method
        ///     to load the new <see cref="LedgerBook" />.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        public async Task<LedgerBook> CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var book = this.ledgerBookFactory.CreateNew();
            book.Name = Path.GetFileNameWithoutExtension(storageKey).Replace('.', ' ');
            book.StorageKey = storageKey;
            book.Modified = DateTime.Now;

            await SaveAsync(book, storageKey);
            return await LoadAsync(storageKey);
        }

        /// <summary>
        ///     Loads the asynchronous.
        /// </summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Data file can not be found:  + storageKey</exception>
        /// <exception cref="DataFormatException">
        ///     Deserialisation Ledger Book file failed, an exception was thrown by the Xaml deserialiser, the file format is
        ///     invalid.
        ///     or
        ///     The Ledger Book has been tampered with, checksum should be  + calculatedChecksum
        /// </exception>
        public async Task<LedgerBook> LoadAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            LedgerBookDto dataEntity;
            try
            {
                this.importUtilities.AbortIfFileDoesntExist(storageKey);
                dataEntity = await LoadXamlFromDiskAsync(storageKey);
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

            if (dataEntity == null)
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

        /// <summary>
        ///     Saves the Ledger Book to the location indicated by the storage key. Any existing Ledger Book at that location will
        ///     be overwritten.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public async Task SaveAsync(LedgerBook book, string storageKey)
        {
            if (book == null)
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

            await SaveDtoToDiskAsync(dataEntity);
        }

        /// <summary>
        ///     Loads the xaml as string.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        /// <summary>
        ///     Loads the xaml from disk asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        protected virtual async Task<LedgerBookDto> LoadXamlFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlAsString(fileName)));
            return result as LedgerBookDto;
        }

        /// <summary>
        ///     Saves the dto to disk asynchronous.
        /// </summary>
        /// <param name="dataEntity">The data entity.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected virtual async Task SaveDtoToDiskAsync([NotNull] LedgerBookDto dataEntity)
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            await WriteToDiskAsync(dataEntity.StorageKey, Serialise(dataEntity));
        }

        /// <summary>
        ///     Serialises the specified data entity.
        /// </summary>
        /// <param name="dataEntity">The data entity.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected virtual string Serialise(LedgerBookDto dataEntity)
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            return XamlServices.Save(dataEntity);
        }

        /// <summary>
        ///     Writes to disk asynchronous.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="data">The data.</param>
        protected virtual async Task WriteToDiskAsync(string fileName, string data)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                using (var file = new StreamWriter(stream))
                {
                    await file.WriteAsync(data);
                }
            }
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