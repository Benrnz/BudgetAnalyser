using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Statement.Data;
using Rees.UserInteraction.Contracts;

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
        private readonly IUserMessageBox userMessageBox;

        public CsvOnDiskStatementModelRepositoryV1(
            [NotNull] IUserMessageBox userMessageBox,
            [NotNull] BankImportUtilities importUtilities,
            [NotNull] ILogger logger,
            [NotNull] BasicMapper<TransactionSetDto, StatementModel> dtoToDomainMapper,
            [NotNull] BasicMapper<StatementModel, TransactionSetDto> domainToDtoMapper)
        {
            if (userMessageBox == null)
            {
                throw new ArgumentNullException("userMessageBox");
            }

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

            this.userMessageBox = userMessageBox;
            this.importUtilities = importUtilities;
            this.logger = logger;
            this.dtoToDomainMapper = dtoToDomainMapper;
            this.domainToDtoMapper = domainToDtoMapper;
        }

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public bool IsValidFile(string fileName)
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName, this.userMessageBox);
            List<string> allLines = ReadLines(fileName, 2).ToList();
            if (!VersionCheck(allLines))
            {
                return false;
            }

            return true;
        }

        public StatementModel Load(string fileName)
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName, this.userMessageBox);

            if (!IsValidFile(fileName))
            {
                throw new VersionNotFoundException("The CSV file is not supported by this version of the Budget Analyser.");
            }

            List<string> allLines = ReadLines(fileName).ToList();
            long totalLines = allLines.LongCount();
            if (totalLines < 2)
            {
                return new StatementModel(this.logger) { FileName = fileName }.LoadTransactions(new List<Transaction>());
            }

            List<TransactionDto> transactions = ReadTransactions(totalLines, allLines);
            TransactionSetDto transactionSet = CreateTransactionSet(fileName, allLines, transactions);

            ValidateChecksumIntegrity(transactionSet);

            return this.dtoToDomainMapper.Map(transactionSet);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Stream and StreamWriter are designed with this pattern in mind")]
        public void Save([NotNull] StatementModel model, string fileName)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }

            var transactionSet = this.domainToDtoMapper.Map(model);
            transactionSet.VersionHash = VersionHash;
            transactionSet.FileName = fileName;
            transactionSet.Checksum = CalculateTransactionCheckSum(transactionSet);

            using (var stream = new FileStream(fileName, FileMode.Create))
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

                        writer.WriteLine(line.ToString());
                    }

                    writer.Flush();
                    writer.Close();
                }
            }

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "StatementModelRepository", ApplicationHookEventArgs.Save));
            }
        }

        protected virtual IEnumerable<string> ReadLines(string fileName)
        {
            return File.ReadLines(fileName);
        }

        protected virtual IEnumerable<string> ReadLines(string fileName, int lines)
        {
            var responseList = new List<string>();
            using (FileStream stream = File.OpenRead(fileName))
            {
                var reader = new StreamReader(stream);
                try
                {
                    for (int index = 0; index < lines; index++)
                    {
                        string line = reader.ReadLine();
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
                throw new FileFormatException("The Budget Analyser file does not have a valid header row.");
            }

            string[] headerSplit = header.Split(',');
            var transactionSet = new TransactionSetDto
            {
                Checksum = this.importUtilities.SafeArrayFetchLong(headerSplit, 3),
                FileName = fileName,
                LastImport = this.importUtilities.SafeArrayFetchDate(headerSplit, 4),
                Transactions = transactions,
                VersionHash = this.importUtilities.SafeArrayFetchString(headerSplit, 1),
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
                        TransactionType = this.importUtilities.SafeArrayFetchString(split, 0),
                        Description = this.importUtilities.SafeArrayFetchString(split, 1),
                        Reference1 = this.importUtilities.SafeArrayFetchString(split, 2),
                        Reference2 = this.importUtilities.SafeArrayFetchString(split, 3),
                        Reference3 = this.importUtilities.SafeArrayFetchString(split, 4),
                        Amount = this.importUtilities.SafeArrayFetchDecimal(split, 5),
                        Date = this.importUtilities.SafeArrayFetchDate(split, 6),
                        BudgetBucketCode = this.importUtilities.SafeArrayFetchString(split, 7),
                        AccountType = this.importUtilities.SafeArrayFetchString(split, 8),
                        Id = this.importUtilities.SafeArrayFetchGuid(split, 9),
                    };
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new FileFormatException("The Budget Analyser file does not have the correct number of columns.", ex);
                }

                if (transaction.Amount == 0 || transaction.Date == DateTime.MinValue || transaction.Id == Guid.Empty)
                {
                    throw new FileFormatException("The Budget Analyser file does not contain the correct data type for Amount and/or Date and/or Id in row " + index + 1);
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
                    () => this.logger.Format("BudgetAnalyser statement file being loaded has an incorrect checksum of: {0}, transactions calculate to: {1}", transactionSet.Checksum, calcTxnCheckSum));
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