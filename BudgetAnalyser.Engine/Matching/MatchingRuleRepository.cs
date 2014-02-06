using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class MatchingRuleRepository : IMatchingRuleRepository
    {
        private readonly IMatchingRuleDataToDomainMapper dataToDomainMapper;
        private readonly IMatchingRuleDomainToDataMapper domainToDataMapper;

        public MatchingRuleRepository([NotNull] IMatchingRuleDataToDomainMapper dataToDomainMapper, [NotNull] IMatchingRuleDomainToDataMapper domainToDataMapper)
        {
            if (dataToDomainMapper == null)
            {
                throw new ArgumentNullException("dataToDomainMapper");
            }

            if (domainToDataMapper == null)
            {
                throw new ArgumentNullException("domainToDataMapper");
            }

            this.dataToDomainMapper = dataToDomainMapper;
            this.domainToDataMapper = domainToDataMapper;
        }

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        public IEnumerable<MatchingRule> LoadRules(string fileName)
        {
            if (!Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            var dataEntities = XamlServices.Load(fileName) as List<DataMatchingRule>;
            if (dataEntities == null)
            {
                throw new FileFormatException("Derserialised MatchingRules are not of type List<DataMatchingRule>");
            }

            return dataEntities.Select(d => this.dataToDomainMapper.Map(d));
        }

        public void SaveRules(IEnumerable<MatchingRule> rules, string fileName)
        {
            IEnumerable<DataMatchingRule> dataEntities = rules.Select(r => this.domainToDataMapper.Map(r));
            XamlServices.Save(fileName, dataEntities.ToList());
        }
    }
}