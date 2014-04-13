using System;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Matching;
using BudgetAnalyser.Engine.Statement;
using GalaSoft.MvvmLight.Command;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.Statement
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AppliedRulesController
    {
        private readonly IMatchMaker matchMacker;
        private readonly IUserMessageBox messageBox;
        private readonly StatementController statementController;

        public AppliedRulesController([NotNull] UiContext uiContext, [NotNull] IMatchMaker matchMacker)
        {
            this.matchMacker = matchMacker;
            if (uiContext == null)
            {
                throw new ArgumentNullException("uiContext");
            }

            if (matchMacker == null)
            {
                throw new ArgumentNullException("matchMacker");
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
            if (this.matchMacker.Match(this.statementController.ViewModel.Statement.Transactions, RulesController.Rules))
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