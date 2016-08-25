using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.BankAccount;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.Statement
{
    /// <summary>
    ///     An importer for ANZ Visa bank statement export.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    internal class AnzVisaStatementImporterV1 : IBankStatementImporter
    {
        private const int Reference1Index = 0;
        private const int TransactionTypeIndex = 1;
        private const int AmountIndex = 2;
        private const int DescriptionIndex = 3;
        private const int DateIndex = 4;
        private const string DebitTransactionType = "D";
        private const string CreditTransactionType = "C";

        private static readonly Dictionary<string, NamedTransaction> TransactionTypes = new Dictionary<string, NamedTransaction>
        {
            { CreditTransactionType, new NamedTransaction("Credit Card Credit") },
            { DebitTransactionType, new NamedTransaction("Credit Card Debit", true) }
        };

        private readonly BankImportUtilities importUtilities;
        private readonly ILogger logger;
        private readonly IReaderWriterSelector readerWriterSelector;

        public AnzVisaStatementImporterV1([NotNull] BankImportUtilities importUtilities, [NotNull] ILogger logger, [NotNull] IReaderWriterSelector readerWriterSelector)
        {
            if (importUtilities == null)
            {
                throw new ArgumentNullException(nameof(importUtilities));
            }

            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            if (readerWriterSelector == null) throw new ArgumentNullException(nameof(readerWriterSelector));

            this.importUtilities = importUtilities;
            this.importUtilities.ConfigureLocale(new CultureInfo("en-NZ"));
            // ANZ importers are NZ specific at this stage.
            this.logger = logger;
            this.readerWriterSelector = readerWriterSelector;
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
            var firstTime = true;
            foreach (var line in await ReadLinesAsync(fileName))
            {
                if (firstTime)
                {
                    // File contains column headers
                    firstTime = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] split = line.Split(',');
                var transactionType = FetchTransactionType(split);
                var transaction = new Transaction
                {
                    Account = account,
                    Reference1 = this.importUtilities.FetchString(split, Reference1Index),
                    TransactionType = transactionType,
                    Description = this.importUtilities.FetchString(split, DescriptionIndex),
                    Date = this.importUtilities.FetchDate(split, DateIndex)
                };
                transaction.Amount = FetchAmount(split, transactionType);
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
            string[] lines = await ReadFirstTwoLinesAsync(fileName);
            if (lines == null || lines.Length != 2 || lines[0].IsNothing() || lines[1].IsNothing())
            {
                return false;
            }

            try
            {
                if (!VerifyColumnHeaderLine(lines[0])) return false;
                if (!VerifyFirstDataLine(lines[1])) return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Reads the lines from the file asynchronously.
        /// </summary>
        protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName)
        {
            var reader = this.readerWriterSelector.SelectReaderWriter(false);
            var allText = await reader.LoadFromDiskAsync(fileName);
            return allText.SplitLines();
        }

        /// <summary>
        ///     Reads a chunk of text asynchronously.
        /// </summary>
        protected virtual async Task<string> ReadTextChunkAsync(string filePath)
        {
            var reader = this.readerWriterSelector.SelectReaderWriter(false);
            return await reader.LoadFirstLinesFromDiskAsync(filePath, 2);
            //using (var sourceStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, false))
            //{
            //    var sb = new StringBuilder();
            //    var buffer = new byte[0x256];
            //    int numRead;
            //    while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            //    {
            //        var text = Encoding.UTF8.GetString(buffer, 0, numRead);
            //        sb.Append(text);
            //        if (text.Contains("\n"))
            //        {
            //            break;
            //        }
            //    }

            //    return sb.ToString();
            //}
        }

        private decimal FetchAmount(string[] array, NamedTransaction transaction)
        {
            var amount = this.importUtilities.FetchDecimal(array, AmountIndex);
            if (transaction.IsDebit)
            {
                amount *= -1;
            }

            return amount;
        }

        private NamedTransaction FetchTransactionType(string[] array)
        {
            var stringType = this.importUtilities.FetchString(array, TransactionTypeIndex);
            if (string.IsNullOrWhiteSpace(stringType))
            {
                return null;
            }

            if (TransactionTypes.ContainsKey(stringType))
            {
                return TransactionTypes[stringType];
            }

            var fullTypeText = stringType;
            var transactionType = new NamedTransaction(fullTypeText, true);
            TransactionTypes.Add(stringType, transactionType);
            return transactionType;
        }

        private async Task<string[]> ReadFirstTwoLinesAsync(string fileName)
        {
            var chunk = await ReadTextChunkAsync(fileName);
            if (chunk.IsNothing())
            {
                return null;
            }
            return chunk.SplitLines(2);
        }

        private static bool VerifyColumnHeaderLine(string line)
        {
            var compareTo = line.EndsWith("\r", StringComparison.OrdinalIgnoreCase) ? line.Remove(line.Length - 1, 1) : line;
            return string.CompareOrdinal(compareTo, "Card,Type,Amount,Details,TransactionDate,ProcessedDate,ForeignCurrencyAmount,ConversionCharge") == 0;
        }

        private bool VerifyFirstDataLine(string line)
        {
            string[] split = line.Split(',');
            var card = this.importUtilities.FetchString(split, Reference1Index);
            if (card.IsSomething())
            {
                if (!char.IsDigit(card.ToCharArray()[0]))
                {
                    return false;
                }
            }

            var amount = this.importUtilities.FetchDecimal(split, AmountIndex);
            if (amount == 0)
            {
                return false;
            }

            var date = this.importUtilities.FetchDate(split, DateIndex);
            if (date == DateTime.MinValue)
            {
                return false;
            }
            return true;
        }
    }
}