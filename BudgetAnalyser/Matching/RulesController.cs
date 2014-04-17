using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.ApplicationState;

namespace BudgetAnalyser.Matching
{
    /// <summary>
    ///     The Controller for <see cref="EditRulesUserControl" /> .
    /// </summary>
    public class RulesController : ControllerBase, IInitializableController, IShowableController
    {
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly IMatchingRuleRepository ruleRepository;
        private RulesGroupedByBucket addNewGroup;
        private MatchingRule addingNewRule;

        private bool doNotUseShown;

        /// <summary>
        ///     Only used if a custom matching rules file is being used. If this is null when the application state has loaded
        ///     message
        ///     arrives nothing will happen, if its a string value this will be used as a full file and path.
        /// </summary>
        private string rulesFileName;

        public RulesController(
            [NotNull] UiContext uiContext,
            [NotNull] IMatchingRuleRepository ruleRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.ruleRepository = ruleRepository;
            NewRuleController = uiContext.NewRuleController;
            RulesGroupedByBucket = new BindingList<RulesGroupedByBucket>();

            MessengerInstance = uiContext.Messenger;
            uiContext.Messenger.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            uiContext.Messenger.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        public ICommand DeleteRuleCommand
        {
            get { return new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand); }
        }

        public ICommand CloseCommand
        {
            get { return new RelayCommand(() => Shown = false); }
        }

        public NewRuleController NewRuleController { get; private set; }

        public IEnumerable<MatchingRule> Rules { get; private set; }
        public BindingList<RulesGroupedByBucket> RulesGroupedByBucket { get; private set; }
        //public BindingList<MatchingRule> Rules { get; private set; }

        public MatchingRule SelectedRule { get; set; }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                if (value == this.doNotUseShown) return;
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public void CreateNewRuleFromTransaction(Transaction transaction)
        {
            if (transaction.BudgetBucket == null || string.IsNullOrWhiteSpace(transaction.BudgetBucket.Code))
            {
                MessageBox.Show("Select a Bucket code first.");
                return;
            }

            NewRuleController.Initialize();
            NewRuleController.Bucket = transaction.BudgetBucket;
            NewRuleController.Description = transaction.Description;
            NewRuleController.Reference1 = transaction.Reference1;
            NewRuleController.Reference2 = transaction.Reference2;
            NewRuleController.Reference3 = transaction.Reference3;
            NewRuleController.TransactionType = transaction.TransactionType.Name;
            NewRuleController.Amount = transaction.Amount;
            NewRuleController.ShowDialog(Rules);

            if (NewRuleController.NewRule != null)
            {
                AddToList(NewRuleController.NewRule);
                //Rules.AddingNew += OnAddingNewGroup;
                //Rules.AddNew();
                //Rules.AddingNew -= OnAddingNewGroup;
                //Rules.EndNew(0);
                //SaveRules();
            }
        }

        public void Initialize()
        {
            try
            {
                LoadRules();
            }
            catch (FileNotFoundException)
            {
                // If file not found occurs here, assume this is the first time the app has run, and create a new one.
                this.ruleRepository.SaveRules(new List<MatchingRule>(), GetFileName());
                LoadRules();
            }
        }

        public void SaveRules()
        {
            this.ruleRepository.SaveRules(Rules, GetFileName());
            Rules = RulesGroupedByBucket.SelectMany(g => g.Rules).OrderBy(r => r.Description);
        }

        protected virtual string GetFileName()
        {
            if (string.IsNullOrWhiteSpace(this.rulesFileName))
            {
                string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                return Path.Combine(path, "MatchingRules.xml");
            }

            return this.rulesFileName;
        }

        protected void LoadRules()
        {
            List<MatchingRule> rules = this.ruleRepository.LoadRules(GetFileName())
                .OrderBy(r => r.Description)
                .ToList();

            Rules = rules;

            IEnumerable<RulesGroupedByBucket> grouped = rules.GroupBy(rule => rule.Bucket)
                .Select(group => new RulesGroupedByBucket(group.Key, group));

            RulesGroupedByBucket = new BindingList<RulesGroupedByBucket>(grouped.ToList());
        }

        private void AddToList(MatchingRule rule)
        {
            RulesGroupedByBucket existingGroup = RulesGroupedByBucket.FirstOrDefault(group => group.Bucket == rule.Bucket);
            if (existingGroup == null)
            {
                this.addNewGroup = new RulesGroupedByBucket(rule.Bucket, new[] { rule });
                RulesGroupedByBucket.AddingNew += OnAddingNewGroup;
                RulesGroupedByBucket.AddNew();
                RulesGroupedByBucket.AddingNew -= OnAddingNewGroup;
                RulesGroupedByBucket.EndNew(0);
                this.addNewGroup = null;
            }
            else
            {
                this.addingNewRule = rule;
                existingGroup.Rules.AddingNew += OnAddingNewRuleToGroup;
                existingGroup.Rules.AddNew();
                existingGroup.Rules.AddingNew -= OnAddingNewRuleToGroup;
                existingGroup.Rules.EndNew(0);
                this.addingNewRule = null;
            }

            SaveRules();
        }

        private bool CanExecuteDeleteRuleCommand()
        {
            return SelectedRule != null;
        }

        private void OnAddingNewGroup(object sender, AddingNewEventArgs addingNewEventArgs)
        {
            addingNewEventArgs.NewObject = this.addNewGroup;
        }

        private void OnAddingNewRuleToGroup(object sender, AddingNewEventArgs addingNewEventArgs)
        {
            addingNewEventArgs.NewObject = this.addingNewRule;
        }

        private void OnApplicationStateLoaded(ApplicationStateLoadedMessage message)
        {
            if (!message.RehydratedModels.ContainsKey(typeof(LastMatchingRulesLoadedV1)))
            {
                return;
            }

            this.rulesFileName = message.RehydratedModels[typeof(LastMatchingRulesLoadedV1)].AdaptModel<string>();
            LoadRules();
        }

        private void OnApplicationStateRequested(ApplicationStateRequestedMessage message)
        {
            var lastRuleSet = new LastMatchingRulesLoadedV1
            {
                Model = this.rulesFileName,
            };
            message.PersistThisModel(lastRuleSet);
        }

        private void OnDeleteRuleCommandExecute()
        {
            if (SelectedRule == null)
            {
                return;
            }

            bool? certainty = this.questionBox.Show("Delete this rule?", "Are you sure?");
            if (certainty != null && certainty.Value)
            {
                RemoveRule();
            }
        }

        private void RemoveRule()
        {
            RulesGroupedByBucket existingGroup = RulesGroupedByBucket.FirstOrDefault(g => g.Bucket == SelectedRule.Bucket);
            if (existingGroup == null)
            {
                return;
            }

            existingGroup.Rules.Remove(SelectedRule);
            SelectedRule = null;
            SaveRules();
        }
    }
}