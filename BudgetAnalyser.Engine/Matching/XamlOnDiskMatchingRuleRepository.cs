using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskMatchingRuleRepository : IMatchingRuleRepository, IApplicationHookEventPublisher
    {
        private readonly IMatchingRuleDataToDomainMapper dataToDomainMapper;
        private readonly IMatchingRuleDomainToDataMapper domainToDataMapper;

        public XamlOnDiskMatchingRuleRepository([NotNull] IMatchingRuleDataToDomainMapper dataToDomainMapper, [NotNull] IMatchingRuleDomainToDataMapper domainToDataMapper)
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

        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        public bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataMatchingRule")]
        public IEnumerable<MatchingRule> LoadRules(string fileName)
        {
            if (!Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            List<DataMatchingRule> dataEntities;
            try
            {
                dataEntities = XamlServices.Load(fileName) as List<DataMatchingRule>;
            }
            catch (Exception ex)
            {
                throw new FileFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            if (dataEntities == null)
            {
                throw new FileFormatException("Derserialised Matching-Rules are not of type List<DataMatchingRule>");
            }

            return dataEntities.Select(d => this.dataToDomainMapper.Map(d));
        }

        public void SaveRules(IEnumerable<MatchingRule> rules, string fileName)
        {
            IEnumerable<DataMatchingRule> dataEntities = rules.Select(r => this.domainToDataMapper.Map(r));
            XamlServices.Save(fileName, dataEntities.ToList());

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "MatchingRuleRepository", ApplicationHookEventArgs.Save));
            }
        }
    }
}