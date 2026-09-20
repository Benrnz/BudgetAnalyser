using System.Text;
using BudgetAnalyser.Engine.BankAccount;

namespace BudgetAnalyser.Engine.Transactions;

/// <summary>
///     An Importer for ASB Everyday Accounts CSV files.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
internal class AsbAccountExtractImporterV1 : CsvBankExtractImporterBase
{
    private const int DateIndex = 0;
    private const int UniqueIdIndex = 1;
    private const int TransactionTypeIndex = 2;
    private const int ChequeNumberIndex = 3;
    private const int PayeeIndex = 4;
    private const int MemoIndex = 5;
    private const int AmountIndex = 6;
    private const int MetadataLineCount = 7;
    private const int ExpectedColumnCount = 7;

    private static readonly Dictionary<string, TransactionType> TransactionTypes = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="AsbAccountExtractImporterV1" /> class.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">
    /// </exception>
    public AsbAccountExtractImporterV1(BankImportUtilities importUtilities, ILogger logger, IReaderWriterSelector readerWriterSelector)
        : base(importUtilities, logger, readerWriterSelector)
    {
    }

    protected override string ExpectedHeaderLine => "Date,Unique Id,Tran Type,Cheque Number,Payee,Memo,Amount";

    /// <summary>
    ///     Load the given file into a <see cref="TransactionsListModel" />.
    /// </summary>
    /// <param name="fileName">The file to load.</param>
    /// <param name="account">
    ///     The account to classify these transactions. This is useful when merging one extract with another. For example,
    ///     merging a cheque account export with visa account export, each can be classified using an account.
    /// </param>
    public override async Task<TransactionsListModel> LoadAsync(string fileName, Account account)
    {
        try
        {
            ImportUtilities.AbortIfFileDoesntExist(fileName);
        }
        catch (FileNotFoundException ex)
        {
            throw new KeyNotFoundException(ex.Message, ex);
        }

        var transactions = new List<Transaction>();
        var lineNumber = 0;
        foreach (var line in await ReadLinesAsync(fileName))
        {
            lineNumber++;

            // Skip metadata lines (lines 1-7)
            if (lineNumber <= MetadataLineCount)
            {
                continue;
            }

            // Skip header line (line 8)
            if (lineNumber == MetadataLineCount + 1)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            transactions.Add(ParseLine(SplitByCommaDelimited(line), account));
        }

        return new TransactionsListModel(Logger) { StorageKey = fileName, LastImport = DateTime.Now }.LoadTransactions(transactions);
    }

    /// <summary>
    ///     Test the given file to see if this importer implementation can read and import it.
    ///     This will open and read some of the contents of the file.
    /// </summary>
    public override async Task<bool> TasteTestAsync(string fileName)
    {
        ImportUtilities.AbortIfFileDoesntExist(fileName);

        var lines = await ReadFirstLinesAsync(fileName);
        if (lines is null || lines.Length < 8 || lines[7].IsNothing())
        {
            return false;
        }

        try
        {
            if (!VerifyMetadataLine(lines[1]))
            {
                return false;
            }

            // Headerline will be at line index 6 if there is an Available Balance line, otherwise line index 5.
            var lineIndex = 6;
            if (!VerifyColumnHeaderLine(lines[lineIndex]))
            {
                lineIndex = 5;
                if (!VerifyColumnHeaderLine(lines[lineIndex]))
                {
                    return false;
                }
            }

            lineIndex++;

            if (!VerifyFirstDataLine(SplitByCommaDelimited(lines[lineIndex])))
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    protected override Transaction ParseLine(string[] split, Account account)
    {
        var transaction = new Transaction
        {
            Account = account,
            Description = ImportUtilities.FetchString(split, MemoIndex),
            Reference1 = ImportUtilities.FetchString(split, PayeeIndex),
            Reference2 = ImportUtilities.FetchString(split, ChequeNumberIndex),
            Reference3 = ImportUtilities.FetchString(split, UniqueIdIndex),
            Amount = ImportUtilities.FetchDecimal(split, AmountIndex),
            Date = ImportUtilities.FetchDate(split, DateIndex)
        };
        transaction.TransactionType = FetchTransactionType(ImportUtilities, TransactionTypes, split, TransactionTypeIndex, transaction.Amount);
        return transaction;
    }

    /// <summary>
    ///     Reads a chunk of text asynchronously.
    /// </summary>
    protected override async Task<string> ReadTextChunkAsync(string filePath)
    {
        var reader = ReaderWriterSelector.SelectReaderWriter(false);
        return await reader.LoadFirstLinesFromDiskAsync(filePath, 9);
    }

    protected override bool VerifyFirstDataLine(string[] split)
    {
        if (split.Length < ExpectedColumnCount)
        {
            return false;
        }

        try
        {
            var date = ImportUtilities.FetchDate(split, DateIndex);
            if (date == DateOnly.MinValue)
            {
                return false;
            }

            var amount = ImportUtilities.FetchDecimal(split, AmountIndex);
            if (amount == 0)
            {
                return false;
            }

            var payee = ImportUtilities.FetchString(split, PayeeIndex);
            if (string.IsNullOrWhiteSpace(payee))
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string[]?> ReadFirstLinesAsync(string fileName)
    {
        var chunk = await ReadTextChunkAsync(fileName);
        return chunk.IsNothing() ? null : chunk.SplitLines(9);
    }

    private static string[] SplitByCommaDelimited(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return [];
        }

        if (line.IndexOf('"') == -1)
        {
            return line.Split(',');
        }

        var sb = new StringBuilder(line.Length);
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                // Handle escaped double quotes "" inside quoted field
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++; // skip the escaped quote
                }
                else
                {
                    inQuotes = !inQuotes;
                    sb.Append('"');
                }
            }
            else if (c == ',' && inQuotes)
            {
                // Replace commas inside quoted fields with semicolons
                sb.Append(';');
            }
            else
            {
                sb.Append(c);
            }
        }

        var processed = sb.ToString();
        return processed.Split(',');
    }

    private static bool VerifyMetadataLine(string line)
    {
        var compareTo = line.EndsWith("\r", StringComparison.OrdinalIgnoreCase) ? line.Remove(line.Length - 1, 1) : line;
        return compareTo.Contains("Bank") && compareTo.Contains("Account");
    }
}
