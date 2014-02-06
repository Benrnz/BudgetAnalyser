using System;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AppliedRulesController
    {
        private readonly IUserMessageBox messageBox;
        private readonly StatementController statementController;

        public AppliedRulesController([NotNull] UiContext uiContext)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            RulesController = uiContext.RulesController;
            this.statementController = uiContext.StatementController;
            this.messageBox = uiContext.UserPrompts.MessageBox;
        }

        public ICommand ApplyRulesCommand
        {
            get { return new RelayCommand(OnApplyRulesCommandExecute, CanExecuteApplyRulesCommand); }
        }

        public ICommand CreateRuleCommand
        {
            get { return new RelayCommand(OnCreateRuleCommandExecute, CanExecuteCreateRuleCommand); }
        }

        public RulesController RulesController { get; private set; }

        public ICommand ShowRulesCommand
        {
            get { return new RelayCommand(OnShowRulesCommandExecute); }
        }

        private bool CanExecuteApplyRulesCommand()
        {
            return RulesController.Rules.Any();
        }

        private bool CanExecuteCreateRuleCommand()
        {
            return this.statementController.SelectedRow != null;
        }

        private void OnApplyRulesCommandExecute()
        {
            bool matchesOccured = false;
            foreach (Transaction transaction in this.statementController.ViewModel.Statement.Transactions)
            {
                if (transaction.BudgetBucket == null || transaction.BudgetBucket.Code == null)
                {
                    foreach (MatchingRule rule in RulesController.Rules)
                    {
                        if (rule.Match(transaction))
                        {
                            transaction.BudgetBucket = rule.Bucket;
                            matchesOccured = true;
                        }
                    }
                }
            }

            if (matchesOccured)
            {
                RulesController.SaveRules();
            }
        }

        private void OnCreateRuleCommandExecute()
        {
            if (this.statementController.SelectedRow == null)
            {
                this.messageBox.Show("No row selected.");
                return;
            }

            RulesController.CreateNewRuleFromTransaction(this.statementController.SelectedRow);
        }

        private void OnShowRulesCommandExecute()
        {
            RulesController.Show();
        }
    }
}