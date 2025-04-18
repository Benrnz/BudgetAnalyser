using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.Engine.Statement;

/// <summary>
///     A set of utilities used when importing bank data
/// </summary>
[AutoRegisterWithIoC]
internal class BankImportUtilities
{
    private readonly ILogger logger;
    private CultureInfo locale;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BankImportUtilities" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public BankImportUtilities(ILogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.locale = CultureInfo.CurrentCulture;
    }

    internal virtual void AbortIfFileDoesntExist(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("File not found.", fileName);
        }
    }

    internal void ConfigureLocale(CultureInfo culture)
    {
        this.locale = culture;
    }

    internal BudgetBucket? FetchBudgetBucket(string[] array, int index, IBudgetBucketRepository bucketRepository)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (bucketRepository is null)
        {
            throw new ArgumentNullException(nameof(bucketRepository));
        }

        var stringType = FetchString(array, index);
        if (string.IsNullOrWhiteSpace(stringType))
        {
            return null;
        }

        stringType = stringType.ToUpperInvariant();

        return bucketRepository.GetByCode(stringType);
    }

    internal DateOnly FetchDate(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        var stringToParse = array[index];
        if (DateOnly.TryParse(stringToParse, this.locale, DateTimeStyles.None, out var result))
        {
            return result;
        }

        this.logger.LogWarning(_ => $"BankImportUtilities: Unable to parse date: {stringToParse}. Attempting to read as a DateTime instead. Will throw if invalid.");

        // Parse as DateTimeOffset to ensure preservation of the original time and timezone.
        if (DateTimeOffset.TryParse(stringToParse, this.locale, DateTimeStyles.None, out var result2))
        {
            var dateOnlyResult = DateOnly.FromDateTime(result2.DateTime);
            this.logger.LogInfo(_ => $"BankImportUtilities: Successfully parsed string '{stringToParse}' as DateTimeOffset: {result2}. DateOnly = {dateOnlyResult}");
            ;
            return dateOnlyResult;
        }

        this.logger.LogError(_ => $"BankImportUtilities: Unable to parse date as pieces: {stringToParse}");
        throw new InvalidDataException($"Expected a date, but file data is invalid: {stringToParse}");
    }

    internal DateOnly FetchDate(ReadOnlySpan<char> array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        // Slice the span starting from the given index
        var span = array.Slice(index);
        var length = 0;

        // Find the next ',' or the end of the span
        foreach (var t in span)
        {
            if (t == ',')
            {
                break;
            }

            length++;
        }

        // Extract the segment up to the next ','
        var segment = span.Slice(0, length);
        if (!DateOnly.TryParse(segment, this.locale, DateTimeStyles.None, out var result))
        {
            var errorMessage = $"BankImportUtilities: Unable to parse DateOnly: {segment.ToString()}";
            this.logger.LogError(_ => errorMessage);
            throw new InvalidDataException(errorMessage);
        }

        return result;
    }

    internal DateTime FetchDateTime(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        var stringToParse = array[index];
        if (!DateTime.TryParse(stringToParse, this.locale, DateTimeStyles.None, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse date: " + stringToParse);
            throw new InvalidDataException("Expected date, but provided data is invalid. " + stringToParse);
        }

        return result;
    }

    internal DateTime FetchDateTime(ReadOnlySpan<char> array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        // Slice the span starting from the given index
        var span = array.Slice(index);
        var length = 0;

        // Find the next ',' or the end of the span
        foreach (var t in span)
        {
            if (t == ',')
            {
                break;
            }

            length++;
        }

        // Extract the segment up to the next ','
        var segment = span.Slice(0, length);
        if (!DateTime.TryParse(segment, this.locale, DateTimeStyles.None, out var result))
        {
            var errorMessage = $"BankImportUtilities: Unable to parse DateTime: {segment.ToString()}";
            this.logger.LogError(_ => errorMessage);
            throw new InvalidDataException(errorMessage);
        }

        return result;
    }

    internal decimal FetchDecimal(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        var stringToParse = array[index];
        if (!decimal.TryParse(stringToParse, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse decimal: " + stringToParse);
            throw new InvalidDataException("Expected decimal, but provided data is invalid. " + stringToParse);
        }

        return result;
    }

    internal decimal FetchDecimal(ReadOnlySpan<char> array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        // Slice the span starting from the given index
        var span = array.Slice(index);
        var length = 0;

        // Find the next ',' or the end of the span
        foreach (var t in span)
        {
            if (t == ',')
            {
                break;
            }

            length++;
        }

        // Extract the segment up to the next ','
        var segment = span.Slice(0, length);
        if (!decimal.TryParse(segment, NumberStyles.Number, this.locale, out var result))
        {
            var errorMessage = $"BankImportUtilities: Unable to parse decimal: {segment.ToString()}";
            this.logger.LogError(_ => errorMessage);
            throw new InvalidDataException(errorMessage);
        }

        return result;
    }

    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Preferable with IoC")]
    internal Guid FetchGuid(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        var stringToParse = array[index];
        if (!Guid.TryParse(stringToParse, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse Guid: " + stringToParse);
            throw new InvalidDataException("Expected Guid, but provided data is invalid. " + stringToParse);
        }

        return result;
    }

    internal Guid FetchGuid(ReadOnlySpan<char> array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        // Slice the span starting from the given index
        var span = array.Slice(index);
        var length = 0;

        // Find the next ',' or the end of the span
        foreach (var t in span)
        {
            if (t == ',')
            {
                break;
            }

            length++;
        }

        // Extract the segment up to the next ','
        var segment = span.Slice(0, length);
        if (!Guid.TryParse(segment, out var result))
        {
            var errorMessage = $"BankImportUtilities: Unable to parse Guid: {segment.ToString()}";
            this.logger.LogError(_ => errorMessage);
            throw new InvalidDataException(errorMessage);
        }

        return result;
    }

    internal long FetchLong(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        var stringToParse = array[index];
        if (!long.TryParse(stringToParse, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse long: " + stringToParse);
            throw new InvalidDataException("Expected long, but provided data is invalid. " + stringToParse);
        }

        return result;
    }

    internal long FetchLong(ReadOnlySpan<char> array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        // Slice the span starting from the given index
        var span = array.Slice(index);
        var length = 0;

        // Find the next ',' or the end of the span
        foreach (var t in span)
        {
            if (t == ',')
            {
                break;
            }

            length++;
        }

        // Extract the segment up to the next ','
        var segment = span.Slice(0, length);
        if (!long.TryParse(segment, out var result))
        {
            var errorMessage = $"BankImportUtilities: Unable to parse long: {segment.ToString()}";
            this.logger.LogError(_ => errorMessage);
            throw new InvalidDataException(errorMessage);
        }

        return result;
    }

    internal string FetchString(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        var result = array[index].Trim();
        var chars = result.ToCharArray();
        return chars.Length > 0 && chars[0] == '"' ? result.Replace("\"", string.Empty) : result;
    }

    internal string FetchString(ReadOnlySpan<char> array, int index)
    {
        if (index < 0 || index >= array.Length)
        {
            ThrowIndexOutOfRangeException(array.Length, index);
        }

        // Slice the span starting from the given index
        var span = array.Slice(index);
        var length = 0;

        // Find the next ',' or the end of the span
        foreach (var t in span)
        {
            if (t == ',')
            {
                break;
            }

            length++;
        }

        // Extract the segment up to the next ','
        var segment = span.Slice(0, length).ToString().Trim();

        // Remove surrounding quotes if present
        if (segment.Length > 0 && segment[0] == '"')
        {
            segment = segment.Replace("\"", string.Empty);
        }

        return segment;
    }

    private static void ThrowIndexOutOfRangeException(int arrayLength, int index)
    {
        throw new UnexpectedIndexException(string.Format(CultureInfo.CurrentCulture, "Index {0} is out of range for array with length {1}.", index, arrayLength));
    }
}
