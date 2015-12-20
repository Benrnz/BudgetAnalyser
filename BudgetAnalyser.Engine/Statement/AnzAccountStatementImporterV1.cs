using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An Importer for ANZ Cheque and Savings Accounts bank statement exports.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AnzAccountStatementImporterV1 : IBankStatementImporter
    {
        private static readonly Dictionary<string, TransactionType> TransactionTypes =
            new Dictionary<string, TransactionType>();

        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnzAccountStatementImporterV1" /> class.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// </exception>
        public AnzAccountStatementImporterV1([NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger)
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
            this.logger = logger;
            this.importUtilities.ConfigureLocale(new CultureInfo("en-NZ"));
                // ANZ importers are NZ specific at this stage.
        }

        /// <summary>
        ///     Load the given file into a <see cref="StatementModel" />.
        /// </summary>
        /// <param name="fileName">The file to load.</param>
        /// <param name="account">
        ///     The account to classify these transactions. This is useful when merging one statement to another. For example,
        ///     merging a cheque account export with visa account export, each can be classified using an account.
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
            foreach (var line in await ReadLinesAsync(fileName))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var split = line.Split(',');
                var transaction = new Transaction
                {
                    Account = account,
                    TransactionType = FetchTransactionType(split, 0),
                    Description = this.importUtilities.FetchString(split, 1),
                    Reference1 = this.importUtilities.FetchString(split, 2),
                    Reference2 = this.importUtilities.FetchString(split, 3),
                    Reference3 = this.importUtilities.FetchString(split, 4),
                    Amount = this.importUtilities.FetchDecimal(split, 5),
                    Date = this.importUtilities.FetchDate(split, 6)
                };
                transactions.Add(transaction);
            }

            var statement = new StatementModel(this.logger)
            {
                StorageKey = fileName,
                LastImport = DateTime.Now
            }.LoadTransactions(transactions);

            return statement;
        }

        /// <summary>
        ///     Test the given file to see if this importer implementation can read and import it.
        ///     This will open and read some of the contents of the file.
        /// </summary>
        public async Task<bool> TasteTestAsync(string fileName)
        {
            this.importUtilities.AbortIfFileDoesntExist(fileName);
            var line = await ReadFirstLineAsync(fileName);
            if (line.IsNothing())
            {
                return false;
            }

            try
            {
                var split = line.Split(',');
                var type = this.importUtilities.FetchString(split, 0);
                if (string.IsNullOrWhiteSpace(type))
                {
                    return false;
                }

                if (char.IsDigit(type.ToCharArray()[0]))
                {
                    return false;
                }

                var amount = this.importUtilities.FetchDecimal(split, 5);
                if (amount == 0)
                {
                    return false;
                }

                var date = this.importUtilities.FetchDate(split, 6);
                if (date == DateTime.MinValue)
                {
                    return false;
                }

                if (split.Length != 8)
                {
                    return false;
                }
            }
            catch (InvalidDataException)
            {
                return false;
            }
            catch (UnexpectedIndexException)
            {
                return false;
            }

            return true;
        }

        private TransactionType FetchTransactionType(string[] array, int index)
        {
            var stringType = this.importUtilities.FetchString(array, index);
            if (stringType.IsNothing())
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

        private async Task<string> ReadFirstLineAsync(string fileName)
        {
            var chunk = await ReadTextChunkAsync(fileName);
            if (chunk.IsNothing())
            {
                return null;
            }

            var position = chunk.IndexOf("\n", StringComparison.OrdinalIgnoreCase);
            if (position > 0)
            {
                return chunk.Substring(0, position);
            }

            return chunk;
        }

        /// <summary>
        ///     Reads the lines from the file asynchronously.
        /// </summary>
        protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            return await this.importUtilities.ReadLinesAsync(fileName);
        }

        /// <summary>
        ///     Reads a chunk of text asynchronously.
        /// </summary>
        protected virtual async Task<string> ReadTextChunkAsync(string filePath)
        {
            using (
                var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, false)
                )
            {
                var sb = new StringBuilder();
                var buffer = new byte[0x128];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, numRead);
                    sb.Append(text);
                    if (text.Contains("\n"))
                    {
                        break;
                    }
                }

                return sb.ToString();
            }
        }
    }
}