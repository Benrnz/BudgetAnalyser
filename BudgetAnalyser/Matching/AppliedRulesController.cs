using System;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Matching
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AppliedRulesController
    {
        private readonly IUserMessageBox messageBox;
        private readonly ITransactionRuleService ruleService;
        private readonly StatementController statementController;

        public AppliedRulesController([NotNull] UiContext uiContext, [NotNull] ITransactionRuleService ruleService)
        {
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (ruleService == null)
            {
                throw new ArgumentNullException("ruleService");
            }

            RulesController = uiContext.RulesController;
            this.ruleService = ruleService;
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
            return RulesController.RulesGroupedByBucket.Any();
        }

        private bool CanExecuteCreateRuleCommand()
        {
            return this.statementController.ViewModel.SelectedRow != null;
        }

        private void OnApplyRulesCommandExecute()
        {
            if (this.ruleService.Match(this.statementController.ViewModel.Statement.Transactions, RulesController.Rules))
            {
                RulesController.SaveRules();
            }
        }

        private void OnCreateRuleCommandExecute()
        {
            if (this.statementController.ViewModel.SelectedRow == null)
            {
                this.messageBox.Show("No row selected.");
                return;
            }

            RulesController.CreateNewRuleFromTransaction(this.statementController.ViewModel.SelectedRow);
        }

        private void OnShowRulesCommandExecute()
        {
            RulesController.Shown = true;
        }
    }
}