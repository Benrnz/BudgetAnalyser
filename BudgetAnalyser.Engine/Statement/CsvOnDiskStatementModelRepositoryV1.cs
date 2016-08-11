using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement.Data;
using JetBrains.Annotations;
using Rees.TangyFruitMapper;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     A repository for the <see cref="StatementModel" /> backed by Csv on local disk storage.
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Statement.IVersionedStatementModelRepository" />
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class CsvOnDiskStatementModelRepositoryV1 : IVersionedStatementModelRepository
    {
        private const string VersionHash = "15955E20-A2CC-4C69-AD42-94D84377FC0C";
        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;
        private readonly IDtoMapper<TransactionSetDto, StatementModel> mapper;
        private readonly IReaderWriterSelector readerWriterSelector;

        public CsvOnDiskStatementModelRepositoryV1(
            [NotNull] BankImportUtilities importUtilities,
            [NotNull] ILogger logger,
            [NotNull] IDtoMapper<TransactionSetDto, StatementModel> mapper,
            [NotNull] IReaderWriterSelector readerWriterSelector)
        {
            if (importUtilities == null)
            {
                throw new ArgumentNullException(nameof(importUtilities));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (mapper == null)
            {
                throw new ArgumentNullException(nameof(mapper));
            }
            if (readerWriterSelector == null) throw new ArgumentNullException(nameof(readerWriterSelector));

            this.importUtilities = importUtilities;
            this.logger = logger;
            this.mapper = mapper;
            this.readerWriterSelector = readerWriterSelector;
        }

        public async Task CreateNewAndSaveAsync(string storageKey)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var newStatement = new StatementModel(this.logger) { StorageKey = storageKey };
            await SaveAsync(newStatement, storageKey, false);
        }

        public async Task<StatementModel> LoadAsync(string storageKey, bool isEncrypted)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            try
            {
                this.importUtilities.AbortIfFileDoesntExist(storageKey);
            }
            catch (FileNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }

            if (!await IsStatementModelAsync(storageKey, isEncrypted))
            {
                throw new NotSupportedException("The CSV file is not supported by this version of the Budget Analyser.");
            }

            List<string> allLines = (await ReadLinesAsync(storageKey, isEncrypted)).ToList();
            var totalLines = allLines.LongCount();
            if (totalLines < 2)
            {
                return new StatementModel(this.logger) { StorageKey = storageKey }.LoadTransactions(new List<Transaction>());
            }

            List<TransactionDto> transactions = ReadTransactions(totalLines, allLines);
            var transactionSet = CreateTransactionSet(storageKey, allLines, transactions);

            ValidateChecksumIntegrity(transactionSet);

            return this.mapper.ToModel(transactionSet);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Stream and StreamWriter are designed with this pattern in mind")]
        public async Task SaveAsync(StatementModel model, string storageKey, bool isEncrypted)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            var transactionSet = this.mapper.ToDto(model);
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

            var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
            using (var stream = writer.CreateWritableStream(storageKey)) 
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    WriteHeader(streamWriter, transactionSet);

                    foreach (var transaction in transactionSet.Transactions)
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

                        line.Append(transaction.Account);
                        line.Append(",");

                        line.Append(transaction.Id);
                        line.Append(",");

                        await streamWriter.WriteLineAsync(line.ToString());
                    }

                    await streamWriter.FlushAsync();
                }
            }
        }

        /// <summary>
        ///     Reads the lines from the file asynchronously.
        /// </summary>
        protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName, bool isEncrypted)
        {
            var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted); 
            var allText = await reader.LoadFromDiskAsync(fileName);
            return allText.SplitLines();
        }

        /// <summary>
        ///     Reads the lines from the file asynchronously.
        /// </summary>
        protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName, int lines, bool isEncrypted)
        {
            var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted); 
            var textData = await reader.LoadFirstLinesFromDiskAsync(fileName, lines);
            string[] firstLines = textData.SplitLines(lines);
            return firstLines;
        }

        internal async Task<bool> IsStatementModelAsync(string storageKey, bool isEncrypted)
        {
            if (storageKey.IsNothing())
            {
                throw new ArgumentNullException(nameof(storageKey));
            }

            this.importUtilities.AbortIfFileDoesntExist(storageKey);
            List<string> allLines = (await ReadLinesAsync(storageKey, 2, isEncrypted)).ToList();
            if (!VersionCheck(allLines))
            {
                return false;
            }

            return true;
        }

        private static long CalculateTransactionCheckSum(TransactionSetDto setDto)
        {
            long txnCheckSum = 37; // prime
            unchecked
            {
                txnCheckSum *= 397; // also prime 
                foreach (var txn in setDto.Transactions)
                {
                    txnCheckSum += (long) txn.Amount * 100;
                    txnCheckSum *= 829;
                }
            }

            return txnCheckSum;
        }

        private TransactionSetDto CreateTransactionSet(string fileName, List<string> allLines, List<TransactionDto> transactions)
        {
            var header = allLines[0];
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
                VersionHash = this.importUtilities.FetchString(headerSplit, 1)
            };
            return transactionSet;
        }

        private List<TransactionDto> ReadTransactions(long totalLines, List<string> allLines)
        {
            var transactions = new List<TransactionDto>();
            for (var index = 1; index < totalLines; index++)
            {
                var line = allLines[index];
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
                        Account = this.importUtilities.FetchString(split, 8),
                        Id = this.importUtilities.FetchGuid(split, 9)
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
                    throw new DataFormatException(
                        "The Budget Analyser file does not contain the correct data type for Date and/or Id in row " +
                        index + 1);
                }

                transactions.Add(transaction);
            }

            return transactions;
        }

        private void ValidateChecksumIntegrity(TransactionSetDto transactionSet)
        {
            var calcTxnCheckSum = CalculateTransactionCheckSum(transactionSet);
            // Ignore a checksum of 1, this is used as a special case to bypass transaction checksum test. Useful for manual manipulation of the statement csv.
            if (transactionSet.Checksum > 1 && transactionSet.Checksum != calcTxnCheckSum)
            {
                this.logger.LogError(l =>
                    l.Format(
                        "BudgetAnalyser statement file being loaded has an incorrect checksum of: {0}, transactions calculate to: {1}",
                        transactionSet.Checksum, calcTxnCheckSum));
                throw new StatementModelChecksumException(
                    calcTxnCheckSum.ToString(CultureInfo.InvariantCulture),
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The statement being loaded, does not match the internal checksum. {0} {1}",
                        calcTxnCheckSum,
                        transactionSet.Checksum));
            }
        }

        private static bool VersionCheck(List<string> allLines)
        {
            var firstLine = allLines[0];
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
    }
}