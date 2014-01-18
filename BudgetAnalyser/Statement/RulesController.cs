using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Matching;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

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

        public RulesController(
            [NotNull] NewRuleController newRuleController,
            [NotNull] IViewLoader maintainRulesViewLoader,
            [NotNull] IUserQuestionBoxYesNo questionBox,
            [NotNull] IMatchingRuleRepository ruleRepository)
        {
            if (newRuleController == null)
            {
                throw new ArgumentNullException("newRuleController");
            }

            if (maintainRulesViewLoader == null)
            {
                throw new ArgumentNullException("maintainRulesViewLoader");
            }
            if (questionBox == null)
            {
                throw new ArgumentNullException("questionBox");
            }

            this.maintainRulesViewLoader = maintainRulesViewLoader;
            this.questionBox = questionBox;
            this.ruleRepository = ruleRepository;
            NewRuleController = newRuleController;
            Rules = new BindingList<MatchingRule>();
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
            LoadRules();
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
            string path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return Path.Combine(path, "MatchingRules.xml");
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