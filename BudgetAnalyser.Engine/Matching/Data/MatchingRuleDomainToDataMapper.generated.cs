using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using Rees.TangyFruitMapper;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;
using System;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.Engine.Matching.Data
{

    [GeneratedCode("1.0", "Tangy Fruit Mapper 2/01/2016 2:10:07 AM UTC")]
    internal partial class MapperMatchingRuleDto2MatchingRule : IDtoMapper<MatchingRuleDto, MatchingRule>
    {

        public virtual MatchingRule ToModel(MatchingRuleDto dto)
        {
            ToModelPreprocessing(dto);
            MatchingRule model = null;
            ModelFactory(dto, ref model);
            if (model is null)
            {
                var constructors = typeof(MatchingRule).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                var constructor = constructors.First(c => c.GetParameters().Length == 0);
                model = (MatchingRule)constructor.Invoke(new Type[] { });
            }
            var modelType = model.GetType();
            var amount1 = dto.Amount;
            model.Amount = amount1;
            var and2 = dto.And;
            model.And = and2;
            var created4 = dto.Created;
            modelType.GetProperty("Created").SetValue(model, created4);
            var description5 = dto.Description;
            model.Description = description5;
            var lastMatch7 = dto.LastMatch;
            modelType.GetProperty("LastMatch").SetValue(model, lastMatch7);
            var matchCount8 = dto.MatchCount;
            modelType.GetProperty("MatchCount").SetValue(model, matchCount8);
            var reference19 = dto.Reference1;
            model.Reference1 = reference19;
            var reference210 = dto.Reference2;
            model.Reference2 = reference210;
            var reference311 = dto.Reference3;
            model.Reference3 = reference311;
            var ruleId12 = dto.RuleId;
            modelType.GetProperty("RuleId").SetValue(model, ruleId12);
            var transactionType13 = dto.TransactionType;
            model.TransactionType = transactionType13;
            ToModelPostprocessing(dto, ref model);
            return model;
        } // End ToModel Method

        public virtual MatchingRuleDto ToDto(MatchingRule model)
        {
            ToDtoPreprocessing(model);
            MatchingRuleDto dto;
            dto = new MatchingRuleDto();
            var amount14 = model.Amount;
            dto.Amount = amount14;
            var and15 = model.And;
            dto.And = and15;
            var created17 = model.Created;
            dto.Created = created17;
            var description18 = model.Description;
            dto.Description = description18;
            var lastMatch19 = model.LastMatch;
            dto.LastMatch = lastMatch19;
            var matchCount20 = model.MatchCount;
            dto.MatchCount = matchCount20;
            var reference121 = model.Reference1;
            dto.Reference1 = reference121;
            var reference222 = model.Reference2;
            dto.Reference2 = reference222;
            var reference323 = model.Reference3;
            dto.Reference3 = reference323;
            var ruleId24 = model.RuleId;
            dto.RuleId = ruleId24;
            var transactionType25 = model.TransactionType;
            dto.TransactionType = transactionType25;
            ToDtoPostprocessing(ref dto, model);
            return dto;
        } // End ToDto Method
        partial void ToModelPreprocessing(MatchingRuleDto dto);
        partial void ToDtoPreprocessing(MatchingRule model);
        partial void ModelFactory(MatchingRuleDto dto, ref MatchingRule model);
        partial void ToModelPostprocessing(MatchingRuleDto dto, ref MatchingRule model);
        partial void ToDtoPostprocessing(ref MatchingRuleDto dto, MatchingRule model);
    } // End Class

} // End Namespace
