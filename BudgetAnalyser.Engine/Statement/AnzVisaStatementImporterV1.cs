using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Annotations;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An importer for ANZ Visa bank statement export.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AnzVisaStatementImporterV1 : IBankStatementImporter
    {
        private static readonly Dictionary<string, NamedTransaction> TransactionTypes = new Dictionary<string, NamedTransaction>();
        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;
        private readonly IUserMessageBox userMessageBox;

        public AnzVisaStatementImporterV1([NotNull] IUserMessageBox userMessageBox, [NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger)
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
                decimal amount;
                NamedTransaction transactionType = FetchTransactionType(split, 1, 2, out amount);
                var transaction = new Transaction
                {
                    AccountType = accountType,
                    Reference1 = this.importUtilities.FetchString(split, 0),
                    TransactionType = transactionType,
                    Amount = amount,
                    Description = this.importUtilities.FetchString(split, 3),
                    Date = this.importUtilities.FetchDate(split, 4),
                };
                transactions.Add(transaction);
            }

            return new StatementModel(this.logger)
            {
                FileName = fileName,
                LastImport = DateTime.Now,
            }.LoadTransactions(transactions);
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
            string card = this.importUtilities.FetchString(split, 0);
            if (string.IsNullOrWhiteSpace(card))
            {
                return false;
            }

            if (!Char.IsDigit(card.ToCharArray()[0]))
            {
                return false;
            }

            decimal amount = this.importUtilities.FetchDecimal(split, 2);
            if (amount == 0)
            {
                return false;
            }

            DateTime date = this.importUtilities.FetchDate(split, 4);
            if (date == DateTime.MinValue)
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
                var buffer = new byte[0x128];
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

        private NamedTransaction FetchTransactionType(string[] array, int transactionTypeindex, int amountIndex, out decimal amount)
        {
            string stringType = this.importUtilities.FetchString(array, transactionTypeindex);
            amount = this.importUtilities.FetchDecimal(array, amountIndex);
            if (string.IsNullOrWhiteSpace(stringType))
            {
                return null;
            }

            if (TransactionTypes.ContainsKey(stringType))
            {
                NamedTransaction cachedTransactionType = TransactionTypes[stringType];
                amount *= cachedTransactionType.Sign;
                return cachedTransactionType;
            }

            string fullTypeText;
            NamedTransaction transactionType;
            if (stringType == "D")
            {
                fullTypeText = "Credit Card Debit";
                transactionType = new NamedTransaction(fullTypeText, -1);
            }
            else if (stringType == "C")
            {
                fullTypeText = "Credit Card Credit";
                transactionType = new NamedTransaction(fullTypeText);
            }
            else
            {
                fullTypeText = stringType;
                transactionType = new NamedTransaction(fullTypeText);
            }

            amount *= transactionType.Sign;
            TransactionTypes.Add(stringType, transactionType);
            return transactionType;
        }

        private string ReadFirstLine(string fileName)
        {
            string chunk = ReadTextChunk(fileName);
            if (string.IsNullOrWhiteSpace(chunk))
            {
                return null;
            }

            int position = chunk.IndexOf("\n", StringComparison.OrdinalIgnoreCase);
            if (position > 0)
            {
                return chunk.Substring(0, position);
            }

            return chunk;
        }
    }
}