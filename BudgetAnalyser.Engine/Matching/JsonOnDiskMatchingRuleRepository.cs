using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Matching;

/// <summary>
///     A Repository to persistently store matching rules in Json format on local disk.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Matching.IMatchingRuleRepository" />
// [AutoRegisterWithIoC(SingleInstance = true)]
internal class JsonOnDiskMatchingRuleRepository(IDtoMapper<MatchingRuleDto, MatchingRule> mapper, ILogger logger, IReaderWriterSelector readerWriterSelector)
    : IMatchingRuleRepository
{
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<MatchingRuleDto, MatchingRule> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReaderWriterSelector readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));

    /// <inheritdoc />
    public IEnumerable<MatchingRule> CreateNew()
    {
        return new List<MatchingRule>();
    }

    /// <inheritdoc />
    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        await SaveAsync(new List<MatchingRule>(), storageKey, false);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<MatchingRule>> LoadAsync(string storageKey, bool isEncrypted)
    {
        if (storageKey.IsNothing())
        {
            throw new KeyNotFoundException("storageKey is blank");
        }

        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Loading Matching Rules from: {storageKey}");
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        if (!reader.FileExists(storageKey))
        {
            throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
        }

        List<MatchingRuleDto> dataEntities;
        try
        {
            dataEntities = await LoadFromDiskAsync(storageKey, isEncrypted);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Deserialisation failed for: {storageKey}");
            throw new DataFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Json deserialiser, the file format is invalid.", ex);
        }

        if (dataEntities is null)
        {
            this.logger.LogWarning(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Deserialised Matching Rules file completed but isn't castable into List<MatchingRuleDto>");
            throw new DataFormatException("Deserialised Matching-Rules are not of type List<MatchingRuleDto>");
        }

        var realModel = dataEntities.Select(d => this.mapper.ToModel(d));
        return Validate(realModel.ToList());
    }

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<MatchingRule> rules, string storageKey, bool isEncrypted)
    {
        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        if (storageKey is null)
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        IEnumerable<MatchingRule> model = Validate(rules.ToList());
        var dataEntities = MapToDto(model);
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Saving Matching Rules to: {storageKey}");
        await SaveToDiskAsync(storageKey, dataEntities, isEncrypted);
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Saved Matching Rules to: {storageKey}");
    }

    protected virtual IEnumerable<MatchingRuleDto> MapToDto(IEnumerable<MatchingRule> model)
    {
        return model.Select(r => this.mapper.ToDto(r));
    }

    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence - this is the type of the rehydrated object")]
    protected virtual async Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = reader.CreateReadableStream(fileName);
        var dto = await JsonSerializer.DeserializeAsync<List<MatchingRuleDto>>(stream, new JsonSerializerOptions());

        return dto ?? throw new DataFormatException("Unable to deserialise Matching Rules into the correct type. File is corrupt.");
    }

    protected virtual async Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities, bool isEncrypted)
    {
        var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await using var stream = writer.CreateWritableStream(fileName);
        await SerialiseAndWriteToStream(stream, dataEntities);
    }

    protected virtual async Task SerialiseAndWriteToStream(Stream stream, IEnumerable<MatchingRuleDto> dataEntities)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        await JsonSerializer.SerializeAsync(stream, dataEntities, options);
    }

    private IList<MatchingRule> Validate(IList<MatchingRule> model)
    {
        // Remove duplicates.
        var duplicatesExist = model.GroupBy(r => r.RuleId).Any(g => g.Count() > 1);
        if (!duplicatesExist)
        {
            return model;
        }

        var knownList = new HashSet<Guid>();
        var indexOfDuplicate = 0;
        bool foundDuplicate;
        do
        {
            foundDuplicate = false;
            for (var index = indexOfDuplicate; index < model.Count; index++)
            {
                if (!knownList.Add(model[index].RuleId))
                {
                    indexOfDuplicate = index;
                    foundDuplicate = true;
                    break;
                }
            }

            if (foundDuplicate)
            {
                var rule = model[indexOfDuplicate];
                this.logger.LogWarning(_ =>
                    $"Duplicate RuleID found and will be removed: {rule.RuleId} {rule.BucketCode} {rule.LastMatch:o} And:{rule.And} {rule.Description} {rule.TransactionType} {rule.Reference1}");
                model.RemoveAt(indexOfDuplicate);
            }
        } while (foundDuplicate);

        return model;
    }
}
