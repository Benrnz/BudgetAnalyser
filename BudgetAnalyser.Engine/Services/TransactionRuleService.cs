using System.Text;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Services;

/// <summary>
///     Implements top level transaction rules functionality.
/// </summary>
/// <seealso cref="BudgetAnalyser.Engine.Services.ITransactionRuleService" />
/// <seealso cref="BudgetAnalyser.Engine.Services.ISupportsModelPersistence" />
[AutoRegisterWithIoC(SingleInstance = true)]
internal class TransactionRuleService(
    IMatchingRuleRepository ruleRepository,
    ILogger logger,
    IMatchmaker matchmaker,
    IMatchingRuleFactory ruleFactory,
    IEnvironmentFolders environmentFolders,
    MonitorableDependencies monitorableDependencies,
    IBudgetBucketRepository bucketRepo)
    : ITransactionRuleService, ISupportsModelPersistence
{
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));
    private readonly IEnvironmentFolders environmentFolders = environmentFolders ?? throw new ArgumentNullException(nameof(environmentFolders));
    private readonly ILogger logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly List<MatchingRule> matchingRules = new();
    private readonly List<RulesGroupedByBucket> matchingRulesGroupedByBucket = new();
    private readonly IMatchmaker matchmaker = matchmaker ?? throw new ArgumentNullException(nameof(matchmaker));
    private readonly MonitorableDependencies monitorableDependencies = monitorableDependencies ?? throw new ArgumentNullException(nameof(monitorableDependencies));
    private readonly IMatchingRuleFactory ruleFactory = ruleFactory ?? throw new ArgumentNullException(nameof(ruleFactory));
    private readonly IMatchingRuleRepository ruleRepository = ruleRepository ?? throw new ArgumentNullException(nameof(ruleRepository));
    private string rulesStorageKey = string.Empty;

    /// <inheritdoc />
    public event EventHandler? Closed;

    /// <inheritdoc />
    public event EventHandler? NewDataSourceAvailable;

    /// <inheritdoc />
    public event EventHandler? Saved;

    /// <inheritdoc />
    public event EventHandler<ValidatingEventArgs>? Saving;

    /// <inheritdoc />
    public event EventHandler<ValidatingEventArgs>? Validating;

    /// <inheritdoc />
    public ApplicationDataType DataType => ApplicationDataType.MatchingRules;

    /// <inheritdoc />
    public int LoadSequence => 50;

    /// <inheritdoc />
    public void Close()
    {
        this.rulesStorageKey = string.Empty;
        this.matchingRulesGroupedByBucket.Clear();
        this.matchingRules.Clear();
        var handler = Closed;
        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task CreateNewAsync(ApplicationDatabase applicationDatabase)
    {
        if (applicationDatabase.MatchingRulesCollectionStorageKey.IsNothing())
        {
            throw new ArgumentNullException(nameof(applicationDatabase));
        }

        await this.ruleRepository.CreateNewAndSaveAsync(applicationDatabase.MatchingRulesCollectionStorageKey);
        await LoadAsync(applicationDatabase);
    }

    /// <inheritdoc />
    public async Task LoadAsync(ApplicationDatabase applicationDatabase)
    {
        this.matchingRules.Clear();
        this.matchingRulesGroupedByBucket.Clear();
        this.rulesStorageKey = applicationDatabase.FullPath(applicationDatabase.MatchingRulesCollectionStorageKey);
        List<MatchingRule> repoRules;
        try
        {
            repoRules = (await this.ruleRepository.LoadAsync(this.rulesStorageKey, applicationDatabase.IsEncrypted))
                .OrderBy(r => r.Description)
                .ToList();
        }
        catch (FileNotFoundException)
        {
            // If file not found occurs here, assume this is the first time the app has run, and create a new one.
            this.rulesStorageKey = await BuildDefaultFileName();
            repoRules = this.ruleRepository.CreateNew().ToList();
        }

        InitialiseTheRulesCollections(repoRules);

        this.monitorableDependencies.NotifyOfDependencyChange<ITransactionRuleService>(this);
        NewDataSourceAvailable?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public async Task SaveAsync(ApplicationDatabase applicationDatabase)
    {
        var messages = new StringBuilder();
        if (ValidateModel(messages))
        {
            // Prefer to use the file name from the applicationDatabase in case it has been changed upstream.
            await this.ruleRepository.SaveAsync(MatchingRules, applicationDatabase.FullPath(applicationDatabase.MatchingRulesCollectionStorageKey), applicationDatabase.IsEncrypted);
        }
        else
        {
            throw new ValidationWarningException("Unable to save matching rules at this time, some data is invalid.\n" + messages);
        }

        this.monitorableDependencies.NotifyOfDependencyChange<ITransactionRuleService>(this);
        Saved?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public void SavePreview()
    {
        var handler = Saving;
        handler?.Invoke(this, new ValidatingEventArgs());
    }

    /// <inheritdoc />
    public bool ValidateModel(StringBuilder messages)
    {
        var handler = Validating;
        handler?.Invoke(this, new ValidatingEventArgs());
        return true;
    }

    /// <inheritdoc />
    public IEnumerable<MatchingRule> MatchingRules => this.matchingRules;

    /// <inheritdoc />
    public IEnumerable<RulesGroupedByBucket> MatchingRulesGroupedByBucket => this.matchingRulesGroupedByBucket;

    /// <inheritdoc />
    public MatchingRule CreateNewRule(string bucketCode, string? description, string[] references, string? transactionTypeName, decimal? amount, bool andMatching)
    {
        var rule = this.ruleFactory.CreateNewRule(bucketCode, description, references, transactionTypeName, amount, andMatching);
        AddRule(rule);
        return rule;
    }

    /// <inheritdoc />
    public SingleUseMatchingRule CreateNewSingleUseRule(string bucketCode, string? description, string[] references, string? transactionTypeName, decimal? amount, bool andMatching)
    {
        var rule = this.ruleFactory.CreateNewSingleUseRule(bucketCode, description, references, transactionTypeName, amount, andMatching);
        AddRule(rule);
        return rule;
    }

    /// <inheritdoc />
    public bool IsRuleSimilar(SimilarMatchedRule rule, DecimalCriteria amount, StringCriteria description, StringCriteria[] references, StringCriteria transactionTypeName)
    {
        IsSimilarRulePreconditions(rule, amount, description, references, transactionTypeName);

        var matchedByResults = new bool[6];
        matchedByResults[0] = amount.IsEqualButNotBlank(rule.Amount);
        matchedByResults[1] = description.IsEqualButNotBlank(rule.Description);
        matchedByResults[2] = references[0].IsEqualButNotBlank(rule.Reference1);
        matchedByResults[3] = references[1].IsEqualButNotBlank(rule.Reference2);
        matchedByResults[4] = references[2].IsEqualButNotBlank(rule.Reference3);
        matchedByResults[5] = transactionTypeName.IsEqualButNotBlank(rule.TransactionType);

        var match = matchedByResults[0];
        match |= matchedByResults[1];
        match |= matchedByResults[2];
        match |= matchedByResults[3];
        match |= matchedByResults[4];
        match |= matchedByResults[5];

        if (match)
        {
            this.logger.LogInfo(l => l.Format("Rule Match: {0} Existing Rule:{1} Criteria:{2}", match, rule, description));
            rule.AmountMatched = matchedByResults[0] && amount.Applicable;
            rule.DescriptionMatched = matchedByResults[1] && description.Applicable;
            rule.Reference1Matched = matchedByResults[2] && references[0].Applicable;
            rule.Reference2Matched = matchedByResults[3] && references[1].Applicable;
            rule.Reference3Matched = matchedByResults[4] && references[2].Applicable;
            rule.TransactionTypeMatched = matchedByResults[5] && transactionTypeName.Applicable;

            return rule.AmountMatched
                   || rule.DescriptionMatched
                   || rule.Reference1Matched
                   || rule.Reference2Matched
                   || rule.Reference3Matched
                   || rule.TransactionTypeMatched;
        }

        return false;
    }

    /// <inheritdoc />
    public bool Match(IEnumerable<Transaction> transactions)
    {
        var matchesMade = this.matchmaker.Match(transactions, MatchingRules);
        this.logger.LogInfo(_ => "TransactionRuleService: Removing any SingleUseRules that have been used.");
        foreach (var rule in MatchingRules.OfType<SingleUseMatchingRule>().ToList())
        {
            if (rule.MatchCount > 0)
            {
                RemoveRule(rule);
            }
        }

        return matchesMade;
    }

    /// <inheritdoc />
    public bool RemoveRule(MatchingRule ruleToRemove)
    {
        if (ruleToRemove is null)
        {
            throw new ArgumentNullException(nameof(ruleToRemove));
        }

        if (string.IsNullOrWhiteSpace(this.rulesStorageKey))
        {
            throw new InvalidOperationException("Unable to remove a matching rule at this time, the service has not yet loaded a matching rule set.");
        }

        var existingGroup = MatchingRulesGroupedByBucket.FirstOrDefault(g => g.Bucket == ruleToRemove.Bucket);
        if (existingGroup is null)
        {
            return false;
        }

        var success1 = existingGroup.Rules.Remove(ruleToRemove);
        var success2 = this.matchingRules.Remove(ruleToRemove);
        var removedRule = ruleToRemove;

        this.logger.LogInfo(_ => "Matching Rule is being Removed: " + removedRule);
        if (!success1)
        {
            this.logger.LogWarning(_ => "Matching Rule was not removed successfully from the Grouped list: " + removedRule);
        }

        if (!success2)
        {
            this.logger.LogWarning(_ => "Matching Rule was not removed successfully from the flat list: " + removedRule);
        }

        return true;
    }

    private void AddRule(MatchingRule ruleToAdd)
    {
        if (ruleToAdd is null)
        {
            throw new ArgumentNullException(nameof(ruleToAdd));
        }

        if (string.IsNullOrWhiteSpace(this.rulesStorageKey))
        {
            throw new InvalidOperationException("Unable to add a matching rule at this time, the service has not yet loaded a matching rule set.");
        }

        // Make sure no rule already exists with this id:
        if (MatchingRules.Any(r => r.RuleId == ruleToAdd.RuleId))
        {
            throw new DuplicateNameException($"Unable to add new matching rule: Rule ID {ruleToAdd.RuleId} already exists in the collection.");
        }

        // Check to see if an existing group object for the desired bucket already exists.
        var existingGroup = MatchingRulesGroupedByBucket.FirstOrDefault(group => group.Bucket == ruleToAdd.Bucket);
        if (existingGroup is null)
        {
            // Create a new group object for this bucket.
            var addNewGroup = new RulesGroupedByBucket(ruleToAdd.Bucket, new[] { ruleToAdd });
            this.matchingRulesGroupedByBucket.Add(addNewGroup);
            this.matchingRules.Add(ruleToAdd);
        }
        else
        {
            // Add to existing group object.
            if (existingGroup.Rules.Contains(ruleToAdd))
            {
                this.logger.LogWarning(_ => "Attempt to add new rule failed. Rule already exists in Grouped collection. " + ruleToAdd);
                return;
            }

            existingGroup.Rules.Add(ruleToAdd);
            if (MatchingRules.Contains(ruleToAdd))
            {
                this.logger.LogWarning(_ => "Attempt to add new rule failed. Rule already exists in main collection. " + ruleToAdd);
                return;
            }

            this.matchingRules.Add(ruleToAdd);
        }

        this.logger.LogInfo(_ => "Matching Rule Added: " + ruleToAdd);
    }

    private async Task<string> BuildDefaultFileName()
    {
        var path = await this.environmentFolders.ApplicationDataFolder();
        return Path.Combine(path, "MatchingRules.xml");
    }

    private void InitialiseTheRulesCollections(List<MatchingRule> repoRules)
    {
        this.matchingRules.AddRange(repoRules);

        IEnumerable<RulesGroupedByBucket> grouped = repoRules.GroupBy(rule => rule.Bucket)
            // this is to prevent showing rules that have a bucket code not currently in the current budget model. Happens when loading the demo or empty budget model.
            .Select(group => new RulesGroupedByBucket(group.Key, group))
            .OrderBy(group => group.Bucket.Code)
            .ToList();

        var allBuckets = this.bucketRepo.Buckets.OrderBy(b => b);
        foreach (var bucket in allBuckets)
        {
            var group = grouped.FirstOrDefault(g => g.Bucket == bucket);
            if (group is null)
            {
                // new bucket found not yet used in the rules, add it
                this.matchingRulesGroupedByBucket.Add(new RulesGroupedByBucket(bucket, new List<MatchingRule>()));
            }
            else
            {
                this.matchingRulesGroupedByBucket.Add(group);
            }
        }
    }

    private static void IsSimilarRulePreconditions(SimilarMatchedRule rule, DecimalCriteria amount, StringCriteria description, StringCriteria[] references, StringCriteria transactionType)
    {
        if (rule is null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        if (amount is null)
        {
            throw new ArgumentNullException(nameof(amount));
        }

        if (description is null)
        {
            throw new ArgumentNullException(nameof(description));
        }

        if (references is null)
        {
            throw new ArgumentNullException(nameof(references));
        }

        if (transactionType is null)
        {
            throw new ArgumentNullException(nameof(transactionType));
        }
    }
}
