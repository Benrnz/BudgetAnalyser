using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class BudgetAnalyserStatementImporterV1 : IVersionedStatementModelImporter
    {
        private const string VersionHash = "15955E20-A2CC-4C69-AD42-94D84377FC0C";

        private static readonly Dictionary<string, TransactionType> TransactionTypes = new Dictionary<string, TransactionType>();
        private readonly IAccountTypeRepository accountTypeRepository;
        private readonly IBudgetBucketRepository bucketRepository;
        private readonly BankImportUtilities importUtilities;
        private readonly IUserMessageBox userMessageBox;

        public BudgetAnalyserStatementImporterV1(
            [NotNull] IAccountTypeRepository accountTypeRepository,
            [NotNull] IUserMessageBox userMessageBox,
            [NotNull] IBudgetBucketRepository bucketRepository,
            [NotNull] BankImportUtilities importUtilities)
        {
            if (accountTypeRepository == null)
            {
                throw new ArgumentNullException("accountTypeRepository");
            }

            if (userMessageBox == null)
            {
                throw new ArgumentNullException("userMessageBox");
            }

            if (bucketRepository == null)
            {
                throw new ArgumentNullException("bucketRepository");
            }

            if (importUtilities == null)
            {
                throw new ArgumentNullException("importUtilities");
            }

            this.accountTypeRepository = accountTypeRepository;
            this.userMessageBox = userMessageBox;
            this.bucketRepository = bucketRepository;
            this.importUtilities = importUtilities;
        }

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

            var transactions = new List<Transaction>();
            List<string> allLines = ReadLines(fileName).ToList();
            long totalLines = allLines.LongCount();
            if (totalLines == 0)
            {
                return null;
            }

            if (!VersionCheck(allLines))
            {
                throw new VersionNotFoundException("The CSV file is not supported by this version of the Budget Analyser.");
            }

            long txnChecksum = ReadTransactionCheckSum(allLines[0]);
            var statementModel = new StatementModel
            {
                FileName = fileName,
                Imported = DateTime.Now,
            };

            if (totalLines == 1)
            {
                return statementModel;
            }

            for (int index = 1; index < totalLines; index++)
            {
                string line = allLines[index];
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] split = line.Split(',');
                var transaction = new Transaction
                {
                    TransactionType = FetchTransactionType(split, 0),
                    Description = this.importUtilities.SafeArrayFetchString(split, 1),
                    Reference1 = this.importUtilities.SafeArrayFetchString(split, 2),
                    Reference2 = this.importUtilities.SafeArrayFetchString(split, 3),
                    Reference3 = this.importUtilities.SafeArrayFetchString(split, 4),
                    Amount = this.importUtilities.SafeArrayFetchDecimal(split, 5),
                    Date = this.importUtilities.SafeArrayFetchDate(split, 6),
                    BudgetBucket = this.importUtilities.FetchBudgetBucket(split, 7, this.bucketRepository),
                    AccountType = FetchAccountType(split, 8),
                    Id = this.importUtilities.SafeArrayFetchGuid(split, 9),
                };
                transactions.Add(transaction);
            }


            statementModel.LoadTransactions(transactions);

            statementModel.DurationInMonths = StatementModel.CalculateDuration(null, statementModel.Transactions);

            long calcTxnCheckSum = CalculateTransactionCheckSum(statementModel);

            // Ignore a checksum of 1, this is used as a special case to bypass transaction checksum test. Useful for manual manipulation of the statement csv.
            if (txnChecksum > 1 && txnChecksum != calcTxnCheckSum)
            {
                throw new StatementModelCheckSumException(
                    calcTxnCheckSum.ToString(CultureInfo.InvariantCulture),
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "The statement being loaded, does not match the internal checksum. {0} {1}",
                        calcTxnCheckSum,
                        txnChecksum));
            }

            return statementModel;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Stream and StreamWriter are designed with this pattern in mind")]
        public void Save(StatementModel model, string fileName)
        {
            IEnumerable<Transaction> transactionsToSave = model.Filtered ? model.AllTransactions : model.Transactions;

            using (var stream = new FileStream(fileName, FileMode.Create))
            {
                using (var writer = new StreamWriter(stream))
                {
                    WriteVersionHash(writer, model);

                    foreach (Transaction transaction in transactionsToSave)
                    {
                        var line = new StringBuilder();
                        line.Append(transaction.TransactionType.Name);
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

                        line.Append(transaction.Date.ToString("dd-MMM-yyyy"));
                        line.Append(",");

                        line.Append(transaction.BudgetBucket == null ? string.Empty : transaction.BudgetBucket.Code);
                        line.Append(",");

                        line.Append(transaction.AccountType == null ? string.Empty : transaction.AccountType.ToString());
                        line.Append(",");

                        line.Append(transaction.Id);
                        line.Append(",");

                        writer.WriteLine(line.ToString());
                    }

                    writer.Flush();
                    writer.Close();
                }
            }
        }

        protected virtual AccountType FetchAccountType(string[] array, int index)
        {
            string stringType = this.importUtilities.SafeArrayFetchString(array, index);
            if (string.IsNullOrWhiteSpace(stringType))
            {
                return null;
            }

            return this.accountTypeRepository.GetOrCreateNew(stringType);
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

        private static long CalculateTransactionCheckSum(StatementModel model)
        {
            long txnCheckSum = 37; // prime
            unchecked
            {
                txnCheckSum *= 397; // also prime 
                foreach (Transaction txn in model.AllTransactions)
                {
                    txnCheckSum += (long) txn.Amount*100;
                    txnCheckSum *= 829;
                }
            }

            return txnCheckSum;
        }

        private static long ReadTransactionCheckSum(string line)
        {
            string[] split = line.Split(',');
            long result;
            if (!long.TryParse(split[3], out result))
            {
                return 1;
            }

            return result;
        }

        private static bool VersionCheck(List<string> allLines)
        {
            string firstLine = allLines[0];
            string[] split = firstLine.Split(',');
            if (split.Length != 4)
            {
                return false;
            }

            if (split[1] != VersionHash)
            {
                return false;
            }

            return true;
        }

        private static void WriteVersionHash(StreamWriter writer, StatementModel model)
        {
            long txnCheckSum = CalculateTransactionCheckSum(model);
            writer.WriteLine("VersionHash,{0},TransactionCheckSum,{1}", VersionHash, txnCheckSum);
        }

        private TransactionType FetchTransactionType(string[] array, int index)
        {
            string stringType = this.importUtilities.SafeArrayFetchString(array, index);
            if (string.IsNullOrWhiteSpace(stringType))
            {
                return null;
            }

            if (TransactionTypes.ContainsKey(stringType))
            {
                return TransactionTypes[stringType];
            }

            var transactionType = new NamedTransaction(stringType);
            TransactionTypes.Add(stringType, transactionType);
            return transactionType;
        }
    }
}