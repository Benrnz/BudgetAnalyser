using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Matching;

/// <summary>
///     A Repository to persistently store matching rules in Json format on local disk.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Matching.IMatchingRuleRepository" />
[AutoRegisterWithIoC(SingleInstance = true)]
internal class JsonOnDiskMatchingRuleRepository(IDtoMapper<MatchingRuleDto, MatchingRule> mapper, ILogger logger, IReaderWriterSelector readerWriterSelector)
    : JsonOnDiskRepositoryBase<List<MatchingRuleDto>>(readerWriterSelector), IMatchingRuleRepository
{
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<MatchingRuleDto, MatchingRule> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

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
        var reader = ReaderWriterSelector.SelectReaderWriter(isEncrypted);
        if (!reader.FileExists(storageKey))
        {
            throw new KeyNotFoundException("Storage key can not be found: " + storageKey);
        }

        List<MatchingRuleDto> dataEntities;
        try
        {
            dataEntities = await LoadJsonFromDiskAsync(storageKey, isEncrypted);
        }
        catch (Exception ex)
        {
            this.logger.LogWarning(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Deserialisation failed for: {storageKey}");
            throw new DataFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Json deserialiser, the file format is invalid.", ex);
        }

        var realModel = dataEntities.Select(d => this.mapper.ToModel(d));
        return PreventDuplicates(realModel.ToList());
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

        IEnumerable<MatchingRule> model = PreventDuplicates(rules.ToList());
        var dataEntities = MapToDto(model);
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Saving Matching Rules to: {storageKey}");
        await SaveToDiskAsync(storageKey, dataEntities, isEncrypted);
        this.logger.LogInfo(_ => $"{nameof(JsonOnDiskMatchingRuleRepository)} Saved Matching Rules to: {storageKey}");
    }

    protected override Exception CreateCorruptFileException()
    {
        return new DataFormatException("Unable to deserialise Matching Rules into the correct type. File is corrupt.");
    }

    protected virtual List<MatchingRuleDto> MapToDto(IEnumerable<MatchingRule> model)
    {
        return model.Select(r => this.mapper.ToDto(r)).ToList();
    }

    private IList<MatchingRule> PreventDuplicates(IList<MatchingRule> model)
    {
        var seen = new HashSet<Guid>();
        var result = new List<MatchingRule>(model.Count);
        foreach (var rule in model)
        {
            if (seen.Add(rule.RuleId))
            {
                result.Add(rule);
                continue;
            }

            this.logger.LogWarning(_ =>
                $"Duplicate RuleID found and will be removed: {rule.RuleId} {rule.BucketCode} {rule.LastMatch:o} And:{rule.And} {rule.Description} {rule.TransactionType} {rule.Reference1}");
        }

        return result;
    }
}
