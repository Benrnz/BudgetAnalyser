using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.BankAccount;

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

        public AnzVisaStatementImporterV1([NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger)
        {
            if (importUtilities == null)
            {
                throw new ArgumentNullException(nameof(importUtilities));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            this.importUtilities = importUtilities;
            this.importUtilities.ConfigureLocale(CultureInfo.CreateSpecificCulture("en-NZ")); // ANZ importers are NZ specific at this stage.
            this.logger = logger;
        }

        /// <summary>
        ///     Load the given file into a <see cref="StatementModel" />.
        /// </summary>
        /// <param name="fileName">The file to load.</param>
        /// <param name="account">
        ///     The account to classify these transactions. This is useful when merging one statement to another. For example,
        ///     merging a cheque account
        ///     export with visa account export, each can be classified using an account.
        /// </param>
        public async Task<StatementModel> LoadAsync(string fileName, Account account)
        {
            try
            {
                this.importUtilities.AbortIfFileDoesntExist(fileName);
            }
            catch (FileNotFoundException ex)
            {
                throw new KeyNotFoundException(ex.Message, ex);
            }

            var transactions = new List<Transaction>();
            foreach (string line in await ReadLinesAsync(fileName))
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
                    Account = account,
                    Reference1 = this.importUtilities.FetchString(split, 0),
                    TransactionType = transactionType,
                    Amount = amount,
                    Description = this.importUtilities.FetchString(split, 3),
                    Date = this.importUtilities.FetchDate(split, 4)
                };
                transactions.Add(transaction);
            }

            return new StatementModel(this.logger)
            {
                StorageKey = fileName,
                LastImport = DateTime.Now
            }.LoadTransactions(transactions);
        }

        /// <summary>
        ///     Test the given file to see if this importer implementation can read and import it.
        ///     This will open and read some of the contents of the file.
        /// </summary>
        public async Task<bool> TasteTestAsync(string fileName)
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName);
            string line = await ReadFirstLineAsync(fileName);
            if (line.IsNothing())
            {
                return false;
            }

            string[] split = line.Split(',');
            string card = this.importUtilities.FetchString(split, 0);
            if (card.IsNothing())
            {
                return false;
            }

            if (!char.IsDigit(card.ToCharArray()[0]))
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

        protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            return await this.importUtilities.ReadLinesAsync(fileName);
        }

        protected virtual async Task<string> ReadTextChunkAsync(string filePath)
        {
            using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, false))
            {
                var sb = new StringBuilder();
                var buffer = new byte[0x128];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
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

        private async Task<string> ReadFirstLineAsync(string fileName)
        {
            string chunk = await ReadTextChunkAsync(fileName);
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