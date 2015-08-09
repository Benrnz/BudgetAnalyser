using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskLedgerBookRepository : ILedgerBookRepository
    {
        private readonly BasicMapper<LedgerBookDto, LedgerBook> dataToDomainMapper;
        private readonly BasicMapper<LedgerBook, LedgerBookDto> domainToDataMapper;
        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;

        public XamlOnDiskLedgerBookRepository(
            [NotNull] BasicMapper<LedgerBookDto, LedgerBook> dataToDomainMapper,
            [NotNull] BasicMapper<LedgerBook, LedgerBookDto> domainToDataMapper,
            [NotNull] ILogger logger,
            [NotNull] BankImportUtilities importUtilities)
        {
            if (dataToDomainMapper == null)
            {
                throw new ArgumentNullException(nameof(dataToDomainMapper));
            }

            if (domainToDataMapper == null)
            {
                throw new ArgumentNullException(nameof(domainToDataMapper));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (importUtilities == null)
            {
                throw new ArgumentNullException(nameof(importUtilities));
            }

            this.dataToDomainMapper = dataToDomainMapper;
            this.domainToDataMapper = domainToDataMapper;
            this.logger = logger;
            this.importUtilities = importUtilities;
        }

        public async Task<LedgerBook> CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var book = new LedgerBook(this.logger)
            {
                Name = Path.GetFileNameWithoutExtension(storageKey).Replace('.', ' '),
                FileName = storageKey,
                Modified = DateTime.Now
            };

            await SaveAsync(book, storageKey);
            return await LoadAsync(storageKey);
        }

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
                throw new DataFormatException("Deserialisation Ledger Book file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            if (dataEntity == null)
            {
                throw new DataFormatException(string.Format(CultureInfo.CurrentCulture, "The specified file {0} is not of type Data-Ledger-Book", storageKey));
            }

            dataEntity.FileName = storageKey;
            LedgerBook book = this.dataToDomainMapper.Map(dataEntity);

            var messages = new StringBuilder();
            if (!book.Validate(messages))
            {
                throw new DataFormatException(messages.ToString());
            }

            if (Math.Abs(dataEntity.Checksum - (-1)) < 0.0001)
            {
                // bypass checksum check - this is to allow intentional manual changes to the file.  This checksum is only trying to prevent
                // bugs in code from breaking the consistency of the file.
            }
            else
            {
                double calculatedChecksum = CalculateChecksum(book);
                if (Math.Abs(calculatedChecksum - dataEntity.Checksum) > 0.0001)
                {
                    throw new DataFormatException("The Ledger Book has been tampered with, checksum should be " + calculatedChecksum);
                }
            }

            return book;
        }

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

            LedgerBookDto dataEntity = this.domainToDataMapper.Map(book);
            book.FileName = storageKey;
            dataEntity.FileName = storageKey;
            dataEntity.Checksum = CalculateChecksum(book);

            await SaveDtoToDiskAsync(dataEntity);
        }

        protected virtual string LoadXamlAsString(string fileName)
        {
            return File.ReadAllText(fileName);
        }

        protected virtual async Task<LedgerBookDto> LoadXamlFromDiskAsync(string fileName)
        {
            object result = null;
            await Task.Run(() => result = XamlServices.Parse(LoadXamlAsString(fileName)));
            return result as LedgerBookDto;
        }

        protected virtual async Task SaveDtoToDiskAsync([NotNull] LedgerBookDto dataEntity)
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            await WriteToDiskAsync(dataEntity.FileName, Serialise(dataEntity));
        }

        protected virtual string Serialise(LedgerBookDto dataEntity)
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException(nameof(dataEntity));
            }

            return XamlServices.Save(dataEntity);
        }

        protected virtual async Task WriteToDiskAsync(string fileName, string data)
        {
            using (var file = new StreamWriter(fileName, false))
            {
                await file.WriteAsync(data);
            }
        }

        private static double CalculateChecksum(LedgerBook dataEntity)
        {
            unchecked
            {
                return dataEntity.Reconciliations.Sum(
                    l =>
                        (double)l.LedgerBalance
                        + l.BankBalanceAdjustments.Sum(b => (double)b.Amount)
                        + l.Entries.Sum(e => (double)e.Balance));
            }
        }
    }
}