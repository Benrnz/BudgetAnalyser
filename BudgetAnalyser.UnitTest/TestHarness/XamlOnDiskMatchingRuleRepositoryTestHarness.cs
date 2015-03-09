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
        public Func<string, string> LoadXamlFromDiskOveride { get; set; }

        public override bool Exists(string storageKey)
        {
            if (ExistsOveride == null)
            {
                return true;
            }

            return ExistsOveride(storageKey);
        }

        protected override string LoadXamlFromDisk(string fileName)
        {
            if (LoadXamlFromDiskOveride == null)
            {
                return base.LoadXamlFromDisk(fileName);
            }

            return LoadXamlFromDiskOveride(fileName);
        }

        protected async override Task<List<MatchingRuleDto>> LoadFromDiskAsync(string fileName)
        {
            if (LoadFromDiskOveride == null)
            {
                return await base.LoadFromDiskAsync(fileName);
            }

            return LoadFromDiskOveride(fileName);
        }

        protected override Task SaveToDiskAsync(string fileName, IEnumerable<MatchingRuleDto> dataEntities)
        {
            if (SaveToDiskOveride == null)
            {
                return Task.Delay(1);
            }

            return Task.Run(() => SaveToDiskOveride(fileName, dataEntities));
        }
    }
}