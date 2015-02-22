using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskMatchingRuleRepositoryTestHarness : XamlOnDiskMatchingRuleRepository
    {
        public XamlOnDiskMatchingRuleRepositoryTestHarness([NotNull] BasicMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper, [NotNull] BasicMapper<MatchingRule, MatchingRuleDto> domainToDataMapper)
            : base(dataToDomainMapper, domainToDataMapper)
        {
        }

        public Func<string, bool> ExistsOveride { get; set; }

        public Func<string, List<MatchingRuleDto>> LoadFromDiskOveride { get; set; }
        public Action<string, IEnumerable<MatchingRuleDto>> SaveToDiskOveride { get; set; }

        public override bool Exists(string storageKey)
        {
            if (ExistsOveride == null)
            {
                return true;
            }

            return ExistsOveride(storageKey);
        }

        protected async override Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName)
        {
            if (LoadFromDiskOveride == null)
            {
                return await base.LoadFromDiskAsync(fileName);
            }

            return LoadFromDiskOveride(fileName);
        }

        protected override void SaveToDisk(string fileName, IEnumerable<MatchingRuleDto> dataEntities)
        {
            if (SaveToDiskOveride == null)
            {
                return;
            }

            SaveToDiskOveride(fileName, dataEntities);
        }
    }
}