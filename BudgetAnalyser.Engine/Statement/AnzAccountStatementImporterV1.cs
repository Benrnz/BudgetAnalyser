using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An Importer for ANZ Cheque and Savings Accounts bank statement exports.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AnzAccountStatementImporterV1 : IBankStatementImporter
    {
        private static readonly Dictionary<string, TransactionType> TransactionTypes = new Dictionary<string, TransactionType>();
        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;
        private readonly IUserMessageBox userMessageBox;

        public AnzAccountStatementImporterV1([NotNull] IUserMessageBox userMessageBox, [NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger)
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

            this.userMessageBox = userMessageBox;
            this.importUtilities = importUtilities;
            this.logger = logger;
        }

        /// <summary>
        ///     Load the given file into a <see cref="StatementModel" />.
        /// </summary>
        /// <param name="fileName">The file to load.</param>
        /// <param name="accountType">
        ///     The account type to classify these transactions. This is useful when merging one statement to another. For example,
        ///     merging a cheque account
        ///     export with visa account export, each can be classified using an account type.
        /// </param>
        public StatementModel Load(string fileName, AccountType accountType)
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName, this.userMessageBox);

            var transactions = new List<Transaction>();
            foreach (string line in ReadLines(fileName))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] split = line.Split(',');
                var transaction = new Transaction
                {
                    AccountType = accountType,
                    TransactionType = FetchTransactionType(split, 0),
                    Description = this.importUtilities.SafeArrayFetchString(split, 1),
                    Reference1 = this.importUtilities.SafeArrayFetchString(split, 2),
                    Reference2 = this.importUtilities.SafeArrayFetchString(split, 3),
                    Reference3 = this.importUtilities.SafeArrayFetchString(split, 4),
                    Amount = this.importUtilities.SafeArrayFetchDecimal(split, 5),
                    Date = this.importUtilities.SafeArrayFetchDate(split, 6),
                };
                transactions.Add(transaction);
            }

            StatementModel statement = new StatementModel(this.logger)
            {
                FileName = fileName,
                Imported = DateTime.Now,
            }.LoadTransactions(transactions);

            return statement;
        }

        /// <summary>
        ///     Test the given file to see if this importer implementation can read and import it.
        ///     This will open and read some of the contents of the file.
        /// </summary>
        public bool TasteTest(string fileName)
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName, this.userMessageBox);
            string line = ReadFirstLine(fileName);
            if (string.IsNullOrWhiteSpace(line))
            {
                return false;
            }

            string[] split = line.Split(',');
            string type = this.importUtilities.SafeArrayFetchString(split, 0);
            if (string.IsNullOrWhiteSpace(type))
            {
                return false;
            }

            if (Char.IsDigit(type.ToCharArray()[0]))
            {
                return false;
            }

            decimal amount = this.importUtilities.SafeArrayFetchDecimal(split, 5);
            if (amount == 0)
            {
                return false;
            }

            DateTime date = this.importUtilities.SafeArrayFetchDate(split, 6);
            if (date == DateTime.MinValue)
            {
                return false;
            }

            if (split.Length != 8)
            {
                return false;
            }

            return true;
        }

        protected virtual IEnumerable<string> ReadLines(string fileName)
        {
            return File.ReadLines(fileName);
        }

        protected virtual string ReadTextChunk(string filePath)
        {
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, false))
            {
                var sb = new StringBuilder();
                var buffer = new byte[0x64];
                int numRead;
                while ((numRead = sourceStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.UTF8.GetString(buffer, 0, numRead);
                    sb.Append(text);
                    if (text.Contains("\n"))
                    {
                        break;
                    }
                }

                return sb.ToString();
            }
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

        private string ReadFirstLine(string fileName)
        {
            string chunk = ReadTextChunk(fileName);
            int position = chunk.IndexOf("\n", StringComparison.InvariantCulture);
            if (position > 0)
            {
                return chunk.Substring(0, position);
            }

            return chunk;
        }
    }
}