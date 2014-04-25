using System;
using System.IO;
using System.Linq;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Ledger
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskLedgerBookRepository : ILedgerBookRepository, IApplicationHookEventPublisher
    {
        private readonly ILedgerDataToDomainMapper dataToDomainMapper;
        private readonly ILedgerDomainToDataMapper domainToDataMapper;
        private readonly ILogger logger;

        public XamlOnDiskLedgerBookRepository(
            [NotNull] ILedgerDataToDomainMapper dataToDomainMapper, 
            [NotNull] ILedgerDomainToDataMapper domainToDataMapper, 
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

        public bool Exists(string fileName)
        {
            return FileExistsOnDisk(fileName);
        }

        public LedgerBook Load(string fileName)
        {
            DataLedgerBook dataEntity;
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
                throw new FileFormatException(string.Format("The specified file {0} is not of type DataLedgerBook", fileName));
            }

            if (dataEntity.Checksum == null)
            {
                // bypass checksum check
            }
            else
            {
                double calculatedChecksum = CalculateChecksum(dataEntity);
                if (calculatedChecksum != dataEntity.Checksum)
                {
                    throw new FileFormatException("The Ledger Book has been tampered with, checksum should be " + calculatedChecksum);
                }
            }

            dataEntity.FileName = fileName;
            return this.dataToDomainMapper.Map(dataEntity);
        }

        public void Save(LedgerBook book)
        {
            Save(book, book.FileName);
        }

        public void Save(LedgerBook book, string fileName)
        {
            DataLedgerBook dataEntity = this.domainToDataMapper.Map(book);
            dataEntity.FileName = fileName;
            dataEntity.Checksum = CalculateChecksum(dataEntity);

            SaveXamlFileToDisk(dataEntity);

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "LedgerBookRepository", ApplicationHookEventArgs.Save));
            }
        }

        public LedgerBook CreateNew(string name, string fileName)
        {
            return new LedgerBook(name, DateTime.Now, fileName, this.logger);
        }

        protected virtual bool FileExistsOnDisk(string fileName)
        {
            return File.Exists(fileName);
        }

        protected virtual DataLedgerBook LoadXamlFromDisk(string fileName)
        {
            return XamlServices.Load(fileName) as DataLedgerBook;
        }

        protected virtual void SaveXamlFileToDisk(DataLedgerBook dataEntity)
        {
            XamlServices.Save(dataEntity.FileName, dataEntity);
        }

        private double CalculateChecksum(DataLedgerBook dataEntity)
        {
            unchecked
            {
                return dataEntity.DatedEntries.Sum(l =>
                    (double)l.BankBalance
                    + l.BankBalanceAdjustments.Sum(b => (double)b.Credit - (double)b.Debit)
                    + l.Entries.Sum(e => (double)e.Balance));
            }
        }
    }
}