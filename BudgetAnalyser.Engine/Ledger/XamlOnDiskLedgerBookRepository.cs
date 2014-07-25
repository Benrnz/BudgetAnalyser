using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskLedgerBookRepository : ILedgerBookRepository, IApplicationHookEventPublisher
    {
        private readonly BasicMapper<LedgerBookDto, LedgerBook> dataToDomainMapper;
        private readonly BasicMapper<LedgerBook, LedgerBookDto> domainToDataMapper;
        private readonly ILogger logger;

        public XamlOnDiskLedgerBookRepository(
            [NotNull] BasicMapper<LedgerBookDto, LedgerBook> dataToDomainMapper,
            [NotNull] BasicMapper<LedgerBook, LedgerBookDto> domainToDataMapper,
            [NotNull] ILogger logger)
        {
            if (dataToDomainMapper == null)
            {
                throw new ArgumentNullException("dataToDomainMapper");
            }

            if (domainToDataMapper == null)
            {
                throw new ArgumentNullException("domainToDataMapper");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.dataToDomainMapper = dataToDomainMapper;
            this.domainToDataMapper = domainToDataMapper;
            this.logger = logger;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public LedgerBook CreateNew(string name, string fileName)
        {
            return new LedgerBook(name, DateTime.Now, fileName, this.logger);
        }

        public bool Exists(string fileName)
        {
            return FileExistsOnDisk(fileName);
        }

        public LedgerBook Load(string fileName)
        {
            LedgerBookDto dataEntity;
            try
            {
                dataEntity = LoadXamlFromDisk(fileName);
            }
            catch (Exception ex)
            {
                throw new FileFormatException("Deserialisation Ledger Book file failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            if (dataEntity == null)
            {
                throw new FileFormatException(string.Format(CultureInfo.CurrentCulture, "The specified file {0} is not of type Data-Ledger-Book", fileName));
            }

            dataEntity.FileName = fileName;
            var book = this.dataToDomainMapper.Map(dataEntity);

            if (dataEntity.Checksum == null)
            {
                // bypass checksum check - this is to allow intentional manual changes to the file.  This checksum is only trying to prevent
                // bugs in code from breaking the consistency of the file.
            }
            else
            {
                double calculatedChecksum = CalculateChecksum(book);
                if (calculatedChecksum != dataEntity.Checksum)
                {
                    throw new FileFormatException("The Ledger Book has been tampered with, checksum should be " + calculatedChecksum);
                }
            }

            return book;
        }

        public void Save([NotNull] LedgerBook book)
        {
            if (book == null)
            {
                throw new ArgumentNullException("book");
            }

            Save(book, book.FileName);
        }

        public void Save(LedgerBook book, string fileName)
        {
            LedgerBookDto dataEntity = this.domainToDataMapper.Map(book);
            dataEntity.FileName = fileName;
            dataEntity.Checksum = CalculateChecksum(book);

            SaveXamlFileToDisk(dataEntity);

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "LedgerBookRepository", ApplicationHookEventArgs.Save));
            }
        }

        protected virtual bool FileExistsOnDisk(string fileName)
        {
            return File.Exists(fileName);
        }

        protected virtual LedgerBookDto LoadXamlFromDisk(string fileName)
        {
            return XamlServices.Load(fileName) as LedgerBookDto;
        }

        protected virtual void SaveXamlFileToDisk([NotNull] LedgerBookDto dataEntity)
        {
            if (dataEntity == null)
            {
                throw new ArgumentNullException("dataEntity");
            }

            XamlServices.Save(dataEntity.FileName, dataEntity);
        }

        private static double CalculateChecksum(LedgerBook dataEntity)
        {
            unchecked
            {
                return dataEntity.DatedEntries.Sum(l =>
                    (double)l.LedgerBalance
                    + l.BankBalanceAdjustments.Sum(b => (double)b.Credit - (double)b.Debit)
                    + l.Entries.Sum(e => (double)e.Balance));
            }
        }
    }
}