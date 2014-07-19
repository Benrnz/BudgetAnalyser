using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskMatchingRuleRepositoryTestHarness : XamlOnDiskMatchingRuleRepository
    {
        public XamlOnDiskMatchingRuleRepositoryTestHarness([NotNull] IMatchingRuleDataToDomainMapper dataToDomainMapper, [NotNull] IMatchingRuleDomainToDataMapper domainToDataMapper)
            : base(dataToDomainMapper, domainToDataMapper)
        {
        }

        public Func<string, bool> ExistsOveride { get; set; }

        public Func<string, List<MatchingRuleDto>> LoadFromDiskOveride { get; set; }
        public Action<string, IEnumerable<MatchingRuleDto>> SaveToDiskOveride { get; set; }

        public override bool Exists(string fileName)
        {
            if (ExistsOveride == null)
            {
                return true;
            }

            return ExistsOveride(fileName);
        }

        protected override List<MatchingRuleDto> LoadFromDisk(string fileName)
        {
            if (LoadFromDiskOveride == null)
            {
                return base.LoadFromDisk(fileName);
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