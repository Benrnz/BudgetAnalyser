using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement.Data;

namespace BudgetAnalyser.Engine.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class CsvOnDiskStatementModelRepositoryV1 : IVersionedStatementModelRepository, IApplicationHookEventPublisher
    {
        private const string VersionHash = "15955E20-A2CC-4C69-AD42-94D84377FC0C";
        private readonly BasicMapper<TransactionSetDto, StatementModel> dtoToDomainMapper;
        private readonly BasicMapper<StatementModel, TransactionSetDto> domainToDtoMapper;

        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;

        public CsvOnDiskStatementModelRepositoryV1(
            [NotNull] BankImportUtilities importUtilities,
            [NotNull] ILogger logger,
            [NotNull] BasicMapper<TransactionSetDto, StatementModel> dtoToDomainMapper,
            [NotNull] BasicMapper<StatementModel, TransactionSetDto> domainToDtoMapper)
        {
            if (importUtilities == null)
            {
                throw new ArgumentNullException("importUtilities");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            if (dtoToDomainMapper == null)
            {
                throw new ArgumentNullException("dtoToDomainMapper");
            }

            if (domainToDtoMapper == null)
            {
                throw new ArgumentNullException("domainToDtoMapper");
            }

            this.importUtilities = importUtilities;
            this.logger = logger;
            this.dtoToDomainMapper = dtoToDomainMapper;
            this.domainToDtoMapper = domainToDtoMapper;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public async Task<bool> IsStatementModelAsync(string storageKey)
        {
            this.importUtilities.AbortIfFileDoesntExist(storageKey);
            List<string> allLines = (await ReadLinesAsync(storageKey, 2)).ToList();
            if (!VersionCheck(allLines))
            {
                return false;
            }

            return true;
        }

        public async Task<StatementModel> LoadAsync(string storageKey)
        {
            try
            {
                this.importUtilities.AbortIfFileDoesntExist(storageKey);
            }
            catch (FileNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }

            if (!(await IsStatementModelAsync(storageKey)))
            {
                throw new NotSupportedException("The CSV file is not supported by this version of the Budget Analyser.");
            }

            List<string> allLines = (await ReadLinesAsync(storageKey)).ToList();
            long totalLines = allLines.LongCount();
            if (totalLines < 2)
            {
                return new StatementModel(this.logger) { StorageKey = storageKey }.LoadTransactions(new List<Transaction>());
            }

            List<TransactionDto> transactions = ReadTransactions(totalLines, allLines);
            TransactionSetDto transactionSet = CreateTransactionSet(storageKey, allLines, transactions);

            ValidateChecksumIntegrity(transactionSet);

            return this.dtoToDomainMapper.Map(transactionSet);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Stream and StreamWriter are designed with this pattern in mind")]
        public async Task SaveAsync([NotNull] StatementModel model, string storageKey)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var transactionSet = this.domainToDtoMapper.Map(model);
            transactionSet.VersionHash = VersionHash;
            transactionSet.StorageKey = storageKey;
            transactionSet.Checksum = CalculateTransactionCheckSum(transactionSet);
            if (model.AllTransactions.Count() != transactionSet.Transactions.Count())
            {
                throw new StatementModelChecksumException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Only {0} out of {1} transactions have been mapped correctly. Aborting the save, to avoid data loss and corruption.", 
                        transactionSet.Transactions.Count, 
                        model.AllTransactions.Count()));
            }

            using (var stream = new FileStream(storageKey, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                using (var writer = new StreamWriter(stream))
                {
                    WriteHeader(writer, transactionSet);

                    foreach (TransactionDto transaction in transactionSet.Transactions)
                    {
                        var line = new StringBuilder();
                        line.Append(transaction.TransactionType);
                        line.Append(",");

                        line.Append(transaction.Description);
                        line.Append(",");

                        line.Append(transaction.Reference1);
                        line.Append(",");

                        line.Append(transaction.Reference2);
                        line.Append(",");

                        line.Append(transaction.Reference3);
                        line.Append(",");

                        line.Append(transaction.Amount);
                        line.Append(",");

                        line.Append(transaction.Date.ToString("O", CultureInfo.InvariantCulture));
                        line.Append(",");

                        line.Append(transaction.BudgetBucketCode);
                        line.Append(",");

                        line.Append(transaction.AccountType);
                        line.Append(",");

                        line.Append(transaction.Id);
                        line.Append(",");

                        await writer.WriteLineAsync(line.ToString());
                    }

                    await writer.FlushAsync();
                    writer.Close();
                }
            }

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "StatementModelRepository", ApplicationHookEventArgs.Save));
            }
        }

        protected async virtual Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            // This will read the entire file then return the complete collection when done. 
            // Given the file size is expected to be relatively small this is the fastest way to do this.  Excessive tasking actually results in poorer performance until file size 
            // becomes large. 
            return await Task.Run(() => File.ReadAllLines(fileName).ToList());
        }

        protected async virtual Task<IEnumerable<string>> ReadLinesAsync(string fileName, int lines)
        {
            var responseList = new List<string>();
            using (var reader = File.OpenText(fileName))
            {
                try
                {
                    for (int index = 0; index < lines; index++)
                    {
                        string line = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            break;
                        }

                        responseList.Add(line);
                    }

                    return responseList;
                }
                catch (IOException)
                {
                    return new List<string>();
                }
            }
        }

        private static long CalculateTransactionCheckSum(TransactionSetDto setDto)
        {
            long txnCheckSum = 37; // prime
            unchecked
            {
                txnCheckSum *= 397; // also prime 
                foreach (TransactionDto txn in setDto.Transactions)
                {
                    txnCheckSum += (long)txn.Amount * 100;
                    txnCheckSum *= 829;
                }
            }

            return txnCheckSum;
        }

        private static bool VersionCheck(List<string> allLines)
        {
            string firstLine = allLines[0];
            string[] split = firstLine.Split(',');
            if (split.Length != 5)
            {
                return false;
            }

            if (split[1] != VersionHash)
            {
                return false;
            }

            return true;
        }

        private static void WriteHeader(StreamWriter writer, TransactionSetDto setDto)
        {
            writer.WriteLine("VersionHash,{0},TransactionCheckSum,{1},{2}", setDto.VersionHash, setDto.Checksum, setDto.LastImport.ToString("O", CultureInfo.InvariantCulture));
        }

        private TransactionSetDto CreateTransactionSet(string fileName, List<string> allLines, List<TransactionDto> transactions)
        {
            string header = allLines[0];
            if (string.IsNullOrWhiteSpace(header))
            {
                throw new DataFormatException("The Budget Analyser file does not have a valid header row.");
            }

            string[] headerSplit = header.Split(',');
            var transactionSet = new TransactionSetDto
            {
                Checksum = this.importUtilities.FetchLong(headerSplit, 3),
                StorageKey = fileName,
                LastImport = this.importUtilities.FetchDate(headerSplit, 4),
                Transactions = transactions,
                VersionHash = this.importUtilities.FetchString(headerSplit, 1),
            };
            return transactionSet;
        }

        private List<TransactionDto> ReadTransactions(long totalLines, List<string> allLines)
        {
            var transactions = new List<TransactionDto>();
            for (int index = 1; index < totalLines; index++)
            {
                string line = allLines[index];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] split = line.Split(',');
                TransactionDto transaction;
                try
                {
                    transaction = new TransactionDto
                    {
                        TransactionType = this.importUtilities.FetchString(split, 0),
                        Description = this.importUtilities.FetchString(split, 1),
                        Reference1 = this.importUtilities.FetchString(split, 2),
                        Reference2 = this.importUtilities.FetchString(split, 3),
                        Reference3 = this.importUtilities.FetchString(split, 4),
                        Amount = this.importUtilities.FetchDecimal(split, 5),
                        Date = this.importUtilities.FetchDate(split, 6),
                        BudgetBucketCode = this.importUtilities.FetchString(split, 7),
                        AccountType = this.importUtilities.FetchString(split, 8),
                        Id = this.importUtilities.FetchGuid(split, 9),
                    };
                }
                catch (InvalidDataException ex)
                {
                    throw new DataFormatException("The Budget Analyser is corrupt. The file has some invalid data in inappropriate columns.", ex);
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new DataFormatException("The Budget Analyser is corrupt. The file does not have the correct number of columns.", ex);
                }

                if (transaction.Date == DateTime.MinValue || transaction.Id == Guid.Empty)
                {
                    // Do not check for Amount == 0 here, sometimes memo transactions can appear with 0.00 or null amounts; which are valid.
                    throw new DataFormatException("The Budget Analyser file does not contain the correct data type for Date and/or Id in row " + index + 1);
                }

                transactions.Add(transaction);
            }

            return transactions;
        }

        private void ValidateChecksumIntegrity(TransactionSetDto transactionSet)
        {
            long calcTxnCheckSum = CalculateTransactionCheckSum(transactionSet);
            // Ignore a checksum of 1, this is used as a special case to bypass transaction checksum test. Useful for manual manipulation of the statement csv.
            if (transactionSet.Checksum > 1 && transactionSet.Checksum != calcTxnCheckSum)
            {
                this.logger.LogError(
                    l => l.Format("BudgetAnalyser statement file being loaded has an incorrect checksum of: {0}, transactions calculate to: {1}", transactionSet.Checksum, calcTxnCheckSum));
                throw new StatementModelChecksumException(
                    calcTxnCheckSum.ToString(CultureInfo.InvariantCulture),
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The statement being loaded, does not match the internal checksum. {0} {1}",
                        calcTxnCheckSum,
                        transactionSet.Checksum));
            }
        }
    }
}