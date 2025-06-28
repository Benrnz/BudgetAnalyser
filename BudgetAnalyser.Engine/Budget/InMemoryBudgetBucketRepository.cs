using BudgetAnalyser.Engine.Budget.Data;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Budget;

/// <summary>
///     A thread-safe in memory implementation of <see cref="IBudgetBucketRepository" />.
///     This repository does not need to be persisted, because it uses the <see cref="BudgetCollection" /> as the source of
///     truth. Thread safety is built in to allow multiple threads to load data from statements asynchronously.
/// </summary>
[AutoRegisterWithIoC(SingleInstance = true)]
public class InMemoryBudgetBucketRepository : IBudgetBucketRepository
{
    private readonly IDtoMapper<BudgetBucketDto, BudgetBucket> mapper;
    private readonly Lock syncRoot = new();
    private Dictionary<string, BudgetBucket> lookupTable = new();

    public InMemoryBudgetBucketRepository(IDtoMapper<BudgetBucketDto, BudgetBucket> mapper)
    {
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        SurplusBucket = new SurplusBucket();
        InitialiseMandatorySpecialBuckets();
    }

    /// <inheritdoc />
    public virtual IEnumerable<BudgetBucket> Buckets => this.lookupTable.Values.OrderBy(b => b.Code).ToList();

    /// <inheritdoc />
    public BudgetBucket SurplusBucket { get; protected init; }

    /// <inheritdoc />
    public virtual FixedBudgetProjectBucket? CreateNewFixedBudgetProject(string bucketCode, string description, decimal fixedBudgetAmount)
    {
        if (string.IsNullOrWhiteSpace(bucketCode))
        {
            throw new ArgumentNullException(nameof(bucketCode));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentNullException(nameof(description));
        }

        if (fixedBudgetAmount <= 0)
        {
            throw new ArgumentException("The fixed budget amount must be greater than zero.", nameof(fixedBudgetAmount));
        }

        var upperCode = FixedBudgetProjectBucket.CreateCode(bucketCode);
        if (IsValidCode(upperCode))
        {
            return null;
        }

        lock (this.syncRoot)
        {
            if (IsValidCode(upperCode))
            {
                return null;
            }

            var bucket = new FixedBudgetProjectBucket(bucketCode, description, fixedBudgetAmount);
            this.lookupTable.Add(upperCode, bucket);
            return bucket;
        }
    }

    /// <inheritdoc />
    public virtual BudgetBucket? GetByCode(string code)
    {
        if (code is null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        var upperCode = code.ToUpperInvariant();
        return IsValidCode(upperCode) ? this.lookupTable[upperCode] : null;
    }

    /// <inheritdoc />
    public virtual BudgetBucket GetOrCreateNew(string code, Func<BudgetBucket> factory)
    {
        if (code is null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        if (factory is null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        var upperCode = code.ToUpperInvariant();
        lock (this.syncRoot)
        {
            if (ContainsKeyInternal(upperCode))
            {
                return this.lookupTable[upperCode];
            }

            var newBucket = factory();
            this.lookupTable.Add(upperCode, newBucket);
            return newBucket;
        }
    }

    /// <inheritdoc />
    public virtual IBudgetBucketRepository Initialise(IEnumerable<BudgetBucketDto> buckets)
    {
        if (buckets is null)
        {
            throw new ArgumentNullException(nameof(buckets));
        }

        lock (this.syncRoot)
        {
            this.lookupTable = buckets
                .Where(dto => dto.Type != BucketDtoType.PayCreditCard && dto.Type != BucketDtoType.Surplus)
                .Select(this.mapper.ToModel)
                .Distinct()
                .ToDictionary(e => e.Code, e => e);

            InitialiseMandatorySpecialBuckets();
        }

        return this;
    }

    /// <inheritdoc />
    public virtual bool IsValidCode(string code)
    {
        return code is null ? throw new ArgumentNullException(nameof(code)) : ContainsKeyInternal(code);
    }

    protected void AddBucket(BudgetBucket bucket)
    {
        if (bucket is null)
        {
            throw new ArgumentNullException(nameof(bucket));
        }

        if (IsValidCode(bucket.Code))
        {
            return;
        }

        lock (this.syncRoot)
        {
            if (IsValidCode(bucket.Code))
            {
                return;
            }

            this.lookupTable.Add(bucket.Code, bucket);
        }
    }

    protected bool ContainsKeyInternal(string code)
    {
        return this.lookupTable.ContainsKey(code.ToUpperInvariant());
    }

    /// <summary>
    ///     Initialises the mandatory special buckets.
    /// </summary>
    protected void InitialiseMandatorySpecialBuckets()
    {
        AddBucket(SurplusBucket);
        AddBucket(new PayCreditCardBucket(PayCreditCardBucket.PayCreditCardCode, "A special bucket to allocate internal transfers."));
    }
}
