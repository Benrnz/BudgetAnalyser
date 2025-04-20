using System.Globalization;
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

    public async Task<StatementModel> LoadAsync(string storageKey, bool isEncrypted, bool tempSwitch = false)
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

        var transactions = await LoadTransactions(storageKey, isEncrypted, tempSwitch);

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

        var transactionSet = MapToDto(model);
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
        await using var stream = CreateWritableStream(storageKey, writer);
        await using var streamWriter = new StreamWriter(stream);
        await WriteToStream(transactionSet, streamWriter);
    }

    protected virtual Stream CreateWritableStream(string storageKey, IFileReaderWriter writer)
    {
        return writer.CreateWritableStream(storageKey);
    }

    protected virtual TransactionSetDto MapToDto(StatementModel model)
    {
        return this.mapper.ToDto(model);
    }

    protected virtual async Task WriteToStream(TransactionSetDto transactionSet, StreamWriter streamWriter)
    {
        WriteHeader(streamWriter, transactionSet);

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

            await streamWriter.WriteLineAsync(line.ToString());
        }

        await streamWriter.FlushAsync();
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

    private async Task<List<TransactionDto>> LoadTransactions(string storageKey, bool isEncrypted, bool tempSwitch = false)
    {
        var lineCount = 0;
        var transactions = new List<TransactionDto>();
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = reader.CreateReadableStream(storageKey);

        await foreach (var lineChars in tempSwitch ? ReadLinesAsync2(stream) : ReadLinesAsync(stream))
        {
            //this.logger.LogInfo(_ => $"{lineCount} {lineChars.ToString()}");
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

    private async IAsyncEnumerable<ReadOnlyMemory<char>> ReadLinesAsync(Stream stream)
    {
        using var streamReader = new StreamReader(stream);

        while (!streamReader.EndOfStream)
        {
            // TODO This needs more work.  Each line is creating a string unnecessarily. Should read in as a Span<char> and return that.
            var line = await streamReader.ReadLineAsync();
            if (line is not null)
            {
                yield return line.AsMemory();
            }
        }
    }

    private async IAsyncEnumerable<ReadOnlyMemory<char>> ReadLinesAsync2(Stream stream)
    {
        // Limitation a line cannot be more than 2048 chars.
        using var streamReader = new StreamReader(stream);
        var startOfLine = true;
        var previousBuffer = new char[1024];
        var leftoverChars = 0;
        var linesReturnedCount = 0;
        var blockBuffer = new char[1024];
        var charsRead = await streamReader.ReadBlockAsync(blockBuffer);
        while (charsRead > 0)
        {
            //this.logger.LogInfo(_ => $"Reading line index {linesReturnedCount}");
            var finishedReading = charsRead < 1024 || streamReader.EndOfStream;
            var lineBeginIndex = 0;
            // Does the buffer start with a return character?
            if (blockBuffer[0] == '\n' || blockBuffer[0] == '\r')
            {
                startOfLine = true;
                // Strip out the return chars
                lineBeginIndex = blockBuffer[1] == '\r' ? 2 : 1;
                // If there was left over chars from the previous chunk read, this means that was a complete line, it was just missing the return char end of line delimiter.
                if (leftoverChars > 0)
                {
                    linesReturnedCount++;
                    yield return TrimEndOfLine(previousBuffer.AsMemory(0, leftoverChars));
                    leftoverChars = 0;
                }
            }

            // Buffer starts with a fresh new line of data.
            if (startOfLine)
            {
                if (finishedReading)
                {
                    // File is so small that it fits in the buffer (1024 chars).
                    linesReturnedCount++;
                    yield return TrimEndOfLine(blockBuffer.AsMemory());
                    break;
                }

                var span = blockBuffer.AsSpan().Slice(lineBeginIndex);
                var endOfLinePosition = span.IndexOf('\n') + lineBeginIndex;
                var length = endOfLinePosition - lineBeginIndex + 1;
                if (leftoverChars <= 0)
                {
                    linesReturnedCount++;
                    yield return TrimEndOfLine(blockBuffer.AsMemory().Slice(lineBeginIndex, length));
                }
                else
                {
                    var lineChars = new char[length + leftoverChars];
                    Array.Copy(previousBuffer, 0, lineChars, 0, leftoverChars);
                    Array.Copy(blockBuffer, lineBeginIndex, lineChars, leftoverChars, length);
                    leftoverChars = 0;
                    linesReturnedCount++;
                    yield return TrimEndOfLine(lineChars.AsMemory());
                }

                startOfLine = false;
                lineBeginIndex = endOfLinePosition + 1;
            }

            // Part way through a read chunk. Find next end of line char.
            while (lineBeginIndex < charsRead)
            {
                // This is suss. Could use .IndexOf('\n') instead within a While loop?
                var span = blockBuffer.AsSpan().Slice(lineBeginIndex);
                var endOfLine = span.IndexOf('\n');
                if (endOfLine < 0)
                {
                    if (finishedReading)
                    {
                        // No EOL found, but we've read all data from the stream, so we assume the remaining chars are a line.
                        endOfLine = span.Length - 1;
                    }
                    else
                    {
                        // No end of line char found, we've run out of characters in the buffer, go read next chunk.
                        break;
                    }
                }

                // The actual index of the end of line char is foundIndex + lineBeginIndex because we sliced the span part way through.
                endOfLine += lineBeginIndex;

                var length = endOfLine - lineBeginIndex + 1;
                if (leftoverChars <= 0)
                {
                    linesReturnedCount++;
                    yield return TrimEndOfLine(blockBuffer.AsMemory().Slice(lineBeginIndex, length));
                }
                else
                {
                    var lineChars = new char[length + leftoverChars];
                    Array.Copy(previousBuffer, 0, lineChars, 0, leftoverChars);
                    Array.Copy(blockBuffer, lineBeginIndex, lineChars, leftoverChars, length);
                    leftoverChars = 0;
                    linesReturnedCount++;
                    yield return TrimEndOfLine(lineChars.AsMemory());
                }

                lineBeginIndex = endOfLine + 1;
                startOfLine = true;
            }

            leftoverChars = charsRead - lineBeginIndex;
            Array.Copy(blockBuffer, lineBeginIndex, previousBuffer, 0, leftoverChars);
            charsRead = await streamReader.ReadBlockAsync(blockBuffer);
        }
    }

    private ReadOnlyMemory<char> TrimEndOfLine(ReadOnlyMemory<char> lineChars)
    {
        var lastIndex = lineChars.Length - 1;
        var span = lineChars.Span;
        if (span[lastIndex] == '\r')
        {
            return lineChars.Slice(0, lastIndex);
        }

        if (span[lastIndex] == '\n')
        {
            if (span[lastIndex - 1] == '\r')
            {
                return lineChars.Slice(0, lastIndex - 1);
            }

            return lineChars.Slice(0, lastIndex);
        }

        return lineChars;
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
