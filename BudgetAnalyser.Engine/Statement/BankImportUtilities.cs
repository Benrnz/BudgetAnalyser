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
            ThrowIndexOutOfRangeException(array, index);
        }

        var stringToParse = array[index];
        if (DateOnly.TryParse(stringToParse, this.locale, DateTimeStyles.None, out var result))
        {
            return result;
        }

        this.logger.LogWarning(_ => $"BankImportUtilities: Unable to parse date: {stringToParse}. Attempting to read as a DateTime instead. Will throw if invalid.");

        if (DateTime.TryParse(stringToParse, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result1))
        {
            this.logger.LogInfo(_ => $"BankImportUtilities: Successfully parsed string '{stringToParse}' as DateTime: {result1}");;
            return DateOnly.FromDateTime(result1);
        }

        throw new InvalidDataException("Expected date, but provided data is invalid. " + stringToParse);
    }

    internal DateTime FetchDateTime(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array, index);
        }

        var stringToParse = array[index];
        if (!DateTime.TryParse(stringToParse, this.locale, DateTimeStyles.None, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse date: " + stringToParse);
            throw new InvalidDataException("Expected date, but provided data is invalid. " + stringToParse);
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
            ThrowIndexOutOfRangeException(array, index);
        }

        var stringToParse = array[index];
        if (!decimal.TryParse(stringToParse, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse decimal: " + stringToParse);
            throw new InvalidDataException("Expected decimal, but provided data is invalid. " + stringToParse);
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
            ThrowIndexOutOfRangeException(array, index);
        }

        var stringToParse = array[index];
        if (!Guid.TryParse(stringToParse, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse Guid: " + stringToParse);
            throw new InvalidDataException("Expected Guid, but provided data is invalid. " + stringToParse);
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
            ThrowIndexOutOfRangeException(array, index);
        }

        var stringToParse = array[index];
        if (!long.TryParse(stringToParse, out var result))
        {
            this.logger.LogError(_ => "BankImportUtilities: Unable to parse long: " + stringToParse);
            throw new InvalidDataException("Expected long, but provided data is invalid. " + stringToParse);
        }

        return result;
    }

    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Preferable with IoC")]
    internal string FetchString(string[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException(nameof(array));
        }

        if (index > array.Length - 1 || index < 0)
        {
            ThrowIndexOutOfRangeException(array, index);
        }

        var result = array[index].Trim();
        var chars = result.ToCharArray();
        return chars.Length > 0 && chars[0] == '"' ? result.Replace("\"", string.Empty) : result;
    }

    private static void ThrowIndexOutOfRangeException(string[] array, int index)
    {
        throw new UnexpectedIndexException(string.Format(CultureInfo.CurrentCulture, "Index {0} is out of range for array with length {1}.", index, array.Length));
    }
}
