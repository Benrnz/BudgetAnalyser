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

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     The Controller for <see cref="MaintainRulesView" /> .
    /// </summary>
    public class RulesController : ControllerBase, IInitializableController
    {
        // BUG Deleting a rule then adding a new one doesnt update the edit rules list to make the new rule appear in the list.
        private readonly IViewLoader maintainRulesViewLoader;
        private readonly IUserQuestionBoxYesNo questionBox;
        private readonly IMatchingRuleRepository ruleRepository;

        /// <summary>
        ///     Only used if a custom matching rules file is being used. If this is null when the application state has loaded
        ///     message
        ///     arrives nothing will happen, if its a string value this will be used as a full file and path.
        /// </summary>
        private string rulesFileName;

        public RulesController(
            [NotNull] UiContext uiContext,
            [NotNull] MaintainRulesViewLoader maintainRulesViewLoader,
            [NotNull] IMatchingRuleRepository ruleRepository)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (maintainRulesViewLoader == null)
            {
                throw new ArgumentNullException("maintainRulesViewLoader");
            }

            this.maintainRulesViewLoader = maintainRulesViewLoader;
            this.questionBox = uiContext.UserPrompts.YesNoBox;
            this.ruleRepository = ruleRepository;
            NewRuleController = uiContext.NewRuleController;
            Rules = new BindingList<MatchingRule>();

            MessengerInstance = uiContext.Messenger;
            uiContext.Messenger.Register<ApplicationStateRequestedMessage>(this, OnApplicationStateRequested);
            uiContext.Messenger.Register<ApplicationStateLoadedMessage>(this, OnApplicationStateLoaded);
        }

        public ICommand DeleteRuleCommand
        {
            get { return new RelayCommand(OnDeleteRuleCommandExecute, CanExecuteDeleteRuleCommand); }
        }

        public NewRuleController NewRuleController { get; private set; }

        public BindingList<MatchingRule> Rules { get; private set; }

        public MatchingRule SelectedRule { get; set; }

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
                Rules.Add(NewRuleController.NewRule);
                SaveRules();
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

        public virtual void SaveRules()
        {
            this.ruleRepository.SaveRules(Rules, GetFileName());
        }

        public void Show()
        {
            this.maintainRulesViewLoader.Show(this);
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

        protected virtual void LoadRules()
        {
            Rules = new BindingList<MatchingRule>(this.ruleRepository.LoadRules(GetFileName())
                .OrderBy(r => r.Description)
                .ToList());
        }

        private bool CanExecuteDeleteRuleCommand()
        {
            return SelectedRule != null;
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
                Rules.Remove(SelectedRule);
                SelectedRule = null;
                SaveRules();
            }
        }
    }
}