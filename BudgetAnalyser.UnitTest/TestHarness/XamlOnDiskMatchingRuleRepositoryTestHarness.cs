using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskMatchingRuleRepositoryTestHarness : XamlOnDiskMatchingRuleRepository
    {
        public XamlOnDiskMatchingRuleRepositoryTestHarness([NotNull] IMatchingRuleDataToDomainMapper dataToDomainMapper, [NotNull] IMatchingRuleDomainToDataMapper domainToDataMapper)
            : base(dataToDomainMapper, domainToDataMapper)
        {
        }

        public Func<string, bool> ExistsOveride { get; set; }

        public Func<string, List<DataMatchingRule>> LoadFromDiskOveride { get; set; }
        public Action<string, IEnumerable<DataMatchingRule>> SaveToDiskOveride { get; set; }

        public override bool Exists(string fileName)
        {
            if (ExistsOveride == null)
            {
                return true;
            }

            return ExistsOveride(fileName);
        }

        protected override List<DataMatchingRule> LoadFromDisk(string fileName)
        {
            if (LoadFromDiskOveride == null)
            {
                return base.LoadFromDisk(fileName);
            }

            return LoadFromDiskOveride(fileName);
        }

        protected override void SaveToDisk(string fileName, IEnumerable<DataMatchingRule> dataEntities)
        {
            if (SaveToDiskOveride == null)
            {
                return;
            }

            SaveToDiskOveride(fileName, dataEntities);
        }
    }
}