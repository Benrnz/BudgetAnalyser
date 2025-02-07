using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.Engine.XUnit.TestHarness;

public class BucketBucketRepoAlwaysFind : InMemoryBudgetBucketRepository
{
    private readonly bool isInitialising;
    private readonly string projectPrefix;

    public BucketBucketRepoAlwaysFind() : base(new MapperBudgetBucketDtoBudgetBucket(new BudgetBucketFactory()))
    {
        this.isInitialising = true;
        SurplusBucket = new SurplusBucket();
        AddBucket(SurplusBucket);
        AddBucket(new PayCreditCardBucket(PayCreditCardBucket.PayCreditCardCode, "A special bucket to allocate internal transfers."));
        this.projectPrefix = string.Format(FixedBudgetProjectBucket.ProjectCodeTemplateWithPrefix, string.Empty);
        this.isInitialising = false;
    }

    public override BudgetBucket GetByCode(string code)
    {
        if (code is null)
        {
            throw new ArgumentNullException(nameof(code));
        }

        if (code == string.Empty)
        {
            return null;
        }

        if (code.StartsWith("INCOME"))
        {
            return GetOrCreateNew(code, () => new IncomeBudgetBucket(code, code));
        }

        if (string.CompareOrdinal(code, SurplusBucket.Code) == 0)
        {
            return SurplusBucket;
        }

        return code.StartsWith(this.projectPrefix)
            ? GetOrCreateNew(code, () => new FixedBudgetProjectBucket(code, code, 100000M))
            : GetOrCreateNew(code, () => new SavedUpForExpenseBucket(code, code));
    }

    public override void Initialise(IEnumerable<BudgetBucketDto> buckets)
    {
        InitialiseMandatorySpecialBuckets();
    }

    public override bool IsValidCode(string code)
    {
        if (this.isInitialising)
        {
            return false;
        }

        return true;
    }
}
