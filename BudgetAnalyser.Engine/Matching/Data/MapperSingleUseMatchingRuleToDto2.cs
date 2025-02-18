using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser.Engine.Matching.Data;

public class MapperSingleUseMatchingRuleToDto2(IBudgetBucketRepository bucketRepo) : IDtoMapper<SingleUseMatchingRuleDto, SingleUseMatchingRule>
{
    private readonly IBudgetBucketRepository bucketRepo = bucketRepo ?? throw new ArgumentNullException(nameof(bucketRepo));

    public SingleUseMatchingRuleDto ToDto(SingleUseMatchingRule model)
    {
        var dto = new SingleUseMatchingRuleDto
        {
            BucketCode = model.BucketCode,
            RuleId = model.RuleId,
            Description = model.Description,
            LastMatch = model.LastMatch,
            MatchCount = model.MatchCount,
            Reference1 = model.Reference1,
            Reference2 = model.Reference2,
            Reference3 = model.Reference3,
            TransactionType = model.TransactionType,
            Amount = model.Amount,
            And = model.And,
            Created = model.Created
        };

        return dto;
    }

    public SingleUseMatchingRule ToModel(SingleUseMatchingRuleDto dto)
    {
        var model = new SingleUseMatchingRule(this.bucketRepo)
        {
            Amount = dto.Amount,
            And = dto.And,
            BucketCode = dto.BucketCode,
            Created = dto.Created ?? DateTime.Now,
            Description = dto.Description,
            LastMatch = dto.LastMatch,
            MatchCount = dto.MatchCount,
            Reference1 = dto.Reference1,
            Reference2 = dto.Reference2,
            Reference3 = dto.Reference3,
            TransactionType = dto.TransactionType,
            RuleId = dto.RuleId ?? Guid.NewGuid()
        };

        return model;
    }
}
