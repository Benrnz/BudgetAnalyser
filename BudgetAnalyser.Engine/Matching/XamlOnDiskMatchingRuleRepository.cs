using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching.Data;

namespace BudgetAnalyser.Engine.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class XamlOnDiskMatchingRuleRepository : IMatchingRuleRepository, IApplicationHookEventPublisher
    {
        private readonly BasicMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper;
        private readonly BasicMapper<MatchingRule, MatchingRuleDto> domainToDataMapper;

        public XamlOnDiskMatchingRuleRepository([NotNull] BasicMapper<MatchingRuleDto, MatchingRule> dataToDomainMapper, [NotNull] BasicMapper<MatchingRule, MatchingRuleDto> domainToDataMapper)
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

        public virtual bool Exists(string fileName)
        {
            return File.Exists(fileName);
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MatchingRuleDto")]
        public IEnumerable<MatchingRule> LoadRules([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (!Exists(fileName))
            {
                throw new FileNotFoundException(fileName);
            }

            List<MatchingRuleDto> dataEntities;
            try
            {
                dataEntities = LoadFromDisk(fileName);
            }
            catch (Exception ex)
            {
                throw new FileFormatException("Deserialisation Matching Rules failed, an exception was thrown by the Xaml deserialiser, the file format is invalid.", ex);
            }

            if (dataEntities == null)
            {
                throw new FileFormatException("Deserialised Matching-Rules are not of type List<MatchingRuleDto>");
            }

            return dataEntities.Select(d => this.dataToDomainMapper.Map(d));
        }

        public void SaveRules([NotNull] IEnumerable<MatchingRule> rules, [NotNull] string fileName)
        {
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }

            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            IEnumerable<MatchingRuleDto> dataEntities = rules.Select(r => this.domainToDataMapper.Map(r));
            SaveToDisk(fileName, dataEntities);

            EventHandler<ApplicationHookEventArgs> handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Repository, "MatchingRuleRepository", ApplicationHookEventArgs.Save));
            }
        }

        protected virtual void SaveToDisk(string fileName, IEnumerable<MatchingRuleDto> dataEntities)
        {
            XamlServices.Save(fileName, dataEntities.ToList());
        }

        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Necessary for persistence - this is the type of the rehydrated object")]
        protected virtual List<MatchingRuleDto> LoadFromDisk(string fileName)
        {
            return XamlServices.Load(fileName) as List<MatchingRuleDto>;
        }
    }
}