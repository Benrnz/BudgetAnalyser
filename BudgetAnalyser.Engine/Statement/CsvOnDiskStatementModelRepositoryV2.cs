﻿using System.Globalization;
using System.Text;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement.Data;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     A repository for the <see cref="StatementModel" /> backed by Csv on local disk storage.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Statement.IVersionedStatementModelRepository" />
// [AutoRegisterWithIoC(SingleInstance = true)]
internal class CsvOnDiskStatementModelRepositoryV2(
    BankImportUtilities importUtilities,
    ILogger logger,
    IDtoMapper<TransactionSetDto, StatementModel> mapper,
    IReaderWriterSelector readerWriterSelector)
    : IVersionedStatementModelRepository
{
    private const string VersionHash = "15955E20-A2CC-4C69-AD42-94D84377FC0C";
    private readonly BankImportUtilities importUtilities = importUtilities ?? throw new ArgumentNullException(nameof(importUtilities));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<TransactionSetDto, StatementModel> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReaderWriterSelector readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));

    private TransactionHeaderDto? transactionHeaderDto;

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

        var transactions = await LoadTransactions(storageKey, isEncrypted);
        if (transactions.Count == 0)
        {
            return new StatementModel(this.logger) { StorageKey = storageKey }.LoadTransactions(new List<Transaction>());
        }

        var transactionSet = CreateTransactionSetDto(transactions);

        ValidateChecksumIntegrity(transactionSet);

        return this.mapper.ToModel(transactionSet);
    }

    public async Task SaveAsync(StatementModel model, string storageKey, bool isEncrypted)
    {
        if (model is null)
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

        var selector = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = selector.CreateWritableStream(storageKey);
        await using var streamWriter = new StreamWriter(stream);
        await WriteToStream(transactionSet, streamWriter);
    }

    /// <summary>
    ///     Reads the lines from the file asynchronously.
    /// </summary>
    protected virtual async Task<IEnumerable<string>> ReadLinesAsync(string fileName, int lines, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        var textData = await reader.LoadFirstLinesFromDiskAsync(fileName, lines);
        var firstLines = textData.SplitLines(lines);
        return firstLines;
    }

    protected virtual async IAsyncEnumerable<ReadOnlyMemory<char>> ReadLinesAsync(string fileName, bool isEncrypted, bool firstLineOnly = false)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = reader.CreateReadableStream(fileName);
        using var streamReader = new StreamReader(stream);

        var lineCount = 0;
        while (!streamReader.EndOfStream)
        {
            var line = await streamReader.ReadLineAsync();
            lineCount++;
            if (line != null && firstLineOnly && lineCount == 1)
            {
                yield return line.AsMemory();
                break;
            }

            if (line != null)
            {
                yield return line.AsMemory();
            }
        }
    }

    protected async Task WriteToStream(TransactionSetDto transactionSet, StreamWriter writer)
    {
        WriteHeader(writer, transactionSet);

        foreach (var transaction in transactionSet.Transactions)
        {
            var line = new StringBuilder();
            line.Append(SanitiseString(transaction.TransactionType, nameof(transaction.TransactionType)));
            line.Append(",");

            line.Append(SanitiseString(transaction.Description, nameof(transaction.Description)));
            line.Append(",");

            line.Append(SanitiseString(transaction.Reference1, nameof(transaction.Reference1)));
            line.Append(",");

            line.Append(SanitiseString(transaction.Reference2, nameof(transaction.Reference2)));
            line.Append(",");

            line.Append(SanitiseString(transaction.Reference3, nameof(transaction.Reference3)));
            line.Append(",");

            line.Append(transaction.Amount);
            line.Append(",");

            line.Append(transaction.Date.ToString("O", CultureInfo.InvariantCulture));
            line.Append(",");

            line.Append(SanitiseString(transaction.BudgetBucketCode, nameof(transaction.BudgetBucketCode)));
            line.Append(",");

            line.Append(SanitiseString(transaction.Account, nameof(transaction.Account)));
            line.Append(",");

            line.Append(transaction.Id);
            line.Append(",");

            await writer.WriteLineAsync(line.ToString());
        }

        await writer.FlushAsync();
    }

    private static long CalculateTransactionCheckSum(TransactionSetDto setDto)
    {
        long txnCheckSum = 37; // prime
        unchecked
        {
            txnCheckSum *= 397; // also prime
            foreach (var txn in setDto.Transactions)
            {
                txnCheckSum += (long)txn.Amount * 100;
                txnCheckSum *= 829;
            }
        }

        return txnCheckSum;
    }

    private TransactionSetDto CreateTransactionSetDto(List<TransactionDto> transactions)
    {
        if (this.transactionHeaderDto == null)
        {
            throw new DataFormatException("The Budget Analyser file does not have a valid header row.");
        }

        var transactionSet = new TransactionSetDto
        {
            Checksum = this.transactionHeaderDto.Checksum,
            StorageKey = this.transactionHeaderDto.StorageKey,
            LastImport = this.transactionHeaderDto.LastImport,
            Transactions = transactions,
            VersionHash = this.transactionHeaderDto.VersionHash
        };
        return transactionSet;
    }

    private async Task<List<TransactionDto>> LoadTransactions(string storageKey, bool isEncrypted)
    {
        var lineCount = 0;
        var transactions = new List<TransactionDto>();
        await foreach (var lineChars in ReadLinesAsync(storageKey, isEncrypted))
        {
            if (lineCount == 0)
            {
                ReadHeaderLine(lineChars, storageKey);
                lineCount++;
                continue;
            }

            var transactionDto = ReadLine(lineChars);
            if (transactionDto == null)
            {
                if (lineCount == 1)
                {
                    return transactions;
                }

                continue;
            }

            if (transactionDto.Date == DateOnly.MinValue || transactionDto.Id == Guid.Empty)
            {
                // Do not check for Amount == 0 here, sometimes memo transactions can appear with 0.00 or null amounts; which are valid.
                throw new DataFormatException($"The Budget Analyser file does not contain the correct data type for Date and/or Id in row {lineCount + 1}");
            }

            transactions.Add(transactionDto);
            lineCount++;
        }

        return transactions;
    }

    private void ReadHeaderLine(ReadOnlyMemory<char> lineChars, string fileName)
    {
        if (lineChars.IsEmpty)
        {
            throw new DataFormatException("The Budget Analyser file does not have a valid header row.");
        }

        var span = lineChars.Span;
        if (span.Count(',') != 4)
        {
            throw new DataFormatException("The Budget Analyser is corrupt. The file does not have the correct number of header columns.");
        }

        try
        {
            this.transactionHeaderDto = new TransactionHeaderDto(span, this.importUtilities, fileName);
            if (this.transactionHeaderDto.VersionHash != VersionHash)
            {
                throw new DataFormatException("The Budget Analyser file does not have the correct version.");
            }
        }
        catch (InvalidDataException ex)
        {
            throw new DataFormatException("The Budget Analyser is corrupt. The file has some invalid data in inappropriate header columns.", ex);
        }
        catch (IndexOutOfRangeException ex)
        {
            throw new DataFormatException("The Budget Analyser is corrupt. The file does not have the correct number of header columns.", ex);
        }
    }

    private TransactionDto? ReadLine(ReadOnlyMemory<char> lineChars)
    {
        if (lineChars.IsEmpty)
        {
            return null;
        }

        var span = lineChars.Span;
        if (span.Count(',') != 10)
        {
            throw new DataFormatException("The Budget Analyser file does not have the correct number of columns.");
        }

        try
        {
            return new TransactionDto
            {
                TransactionType = this.importUtilities.FetchString(span, 0),
                Description = this.importUtilities.FetchString(span, 1),
                Reference1 = this.importUtilities.FetchString(span, 2),
                Reference2 = this.importUtilities.FetchString(span, 3),
                Reference3 = this.importUtilities.FetchString(span, 4),
                Amount = this.importUtilities.FetchDecimal(span, 5),
                Date = this.importUtilities.FetchDate(span, 6),
                BudgetBucketCode = this.importUtilities.FetchString(span, 7),
                Account = this.importUtilities.FetchString(span, 8),
                Id = this.importUtilities.FetchGuid(span, 9)
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
    }

    private string? SanitiseString(string? data, string property)
    {
        if (string.IsNullOrWhiteSpace(data))
        {
            return data;
        }

        var result = data.Replace(",", string.Empty);
        if (result.Length != data.Length)
        {
            this.logger.LogWarning(l => l.Format("'{0}' contains commas, and they have been stripped out. [{1}] -> [{2}]", property, data, result));
        }

        return result;
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
                string.Format(
                    CultureInfo.CurrentCulture,
                    "The statement being loaded, does not match the internal checksum. {0} {1}",
                    calcTxnCheckSum,
                    transactionSet.Checksum));
        }
    }

    private static void WriteHeader(StreamWriter writer, TransactionSetDto setDto)
    {
        writer.WriteLine("VersionHash,{0},TransactionCheckSum,{1},{2}", setDto.VersionHash, setDto.Checksum, setDto.LastImport.ToString("O", CultureInfo.InvariantCulture));
    }
}
