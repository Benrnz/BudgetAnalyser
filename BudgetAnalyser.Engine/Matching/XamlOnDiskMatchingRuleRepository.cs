using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.Engine.Matching.Data;
using BudgetAnalyser.Engine.Persistence;
using Portable.Xaml;

namespace BudgetAnalyser.Engine.Matching;

/// <summary>
///     A Repository to persistently store matching rules in Xaml format on local disk.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Matching.IMatchingRuleRepository" />
[AutoRegisterWithIoC(SingleInstance = true)]
internal class XamlOnDiskMatchingRuleRepository(IDtoMapper<MatchingRuleDto, MatchingRule> mapper, ILogger logger, IReaderWriterSelector readerWriterSelector)
    : IMatchingRuleRepository
{
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDtoMapper<MatchingRuleDto, MatchingRule> mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IReaderWriterSelector readerWriterSelector = readerWriterSelector ?? throw new ArgumentNullException(nameof(readerWriterSelector));

    public IEnumerable<MatchingRule> CreateNew()
    {
        return new List<MatchingRule>();
    }

    public async Task CreateNewAndSaveAsync(string storageKey)
    {
        if (storageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(storageKey));
        }

        await SaveAsync(new List<MatchingRule>(), storageKey, false);
    }

    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MatchingRuleDto")]
    public async Task<IEnumerable<MatchingRule>> LoadAsync(string storageKey, bool isEncrypted)
    {
        if (storageKey.IsNothing())
        {
            throw new KeyNotFoundException("storageKey is blank");
        }

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
            throw new DataFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
        }

        if (dataEntities is null)
        {
            throw new DataFormatException("Deserialised Matching-Rules are not of type List<MatchingRuleDto>");
        }

        var realModel = dataEntities.Select(d => this.mapper.ToModel(d));
        return Validate(realModel.ToList());
    }

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
        var dataEntities = model.Select(r => this.mapper.ToDto(r));
        await SaveToDiskAsync(storageKey, dataEntities, isEncrypted);
    }

    // ReSharper disable once VirtualMemberNeverOverridden.Global // Used in unit testing
    protected virtual object Deserialise(string xaml)
    {
        return XamlServices.Parse(xaml);
    }

    [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence - this is the type of the rehydrated object")]
    protected virtual async Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName, bool isEncrypted)
    {
        var reader = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        var result = await reader.LoadFromDiskAsync(fileName);
        return (List<MatchingRuleDto>)Deserialise(result);
    }

    protected virtual async Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities, bool isEncrypted)
    {
        var writer = this.readerWriterSelector.SelectReaderWriter(isEncrypted);
        await writer.WriteToDiskAsync(fileName, Serialise(dataEntities));
    }

    protected virtual string Serialise(IEnumerable<MatchingRuleDto> dataEntity)
    {
        return dataEntity is null ? throw new ArgumentNullException(nameof(dataEntity)) : XamlServices.Save(dataEntity.ToList());
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
