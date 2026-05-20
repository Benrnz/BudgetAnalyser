using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookViewModel : ObservableRecipient
    {
        private IBudgetCurrencyContext? doNotUseCurrentBudget;
        private StatementModel? doNotUseCurrentStatement;
        private Engine.Ledger.LedgerBook? doNotUseLedgerBook;
        private LedgerEntryLine? doNotUseNewLedgerLine;

        public bool AddNewReconciliationIsEnabled =>
            // Decided not to validate budget here, budget for dates is a more complicated decision / validation for the engine.
            CurrentStatement is not null && LedgerBook is not null;

        public Engine.Ledger.LedgerBook? LedgerBook
        {
            get => this.doNotUseLedgerBook;

            set
            {
                if (Equals(value, this.doNotUseLedgerBook))
                {
                    return;
                }

                this.doNotUseLedgerBook = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoLedgerBookLoaded));
                OnPropertyChanged(nameof(AddNewReconciliationIsEnabled));
            }
        }

        /// <summary>
        ///     Is only referring to finding a budget based on DateTime.Today!  Do not use for reconciliation purposes or validation. Delegate down to the engine. 
        /// </summary>
        public bool NoBudgetLoaded => CurrentBudget is null;
        public bool NoLedgerBookLoaded => LedgerBook is null;
        public bool NoStatementLoaded => CurrentStatement is null;

        /// <summary>
        ///     CurrentBudget is not used for reconciliation purposes, for recon purposes this needs to find the effective budget for the recon date, NOT the current budget.
        ///     CurrentBudget should only be used for UI purposes such as an indication of current budgeted amount for something etc. 
        /// </summary>
        internal IBudgetCurrencyContext? CurrentBudget
        {
            get => this.doNotUseCurrentBudget;
            set
            {
                this.doNotUseCurrentBudget = value;
                OnPropertyChanged(nameof(NoBudgetLoaded));
            }
        }

        internal StatementModel? CurrentStatement
        {
            get => this.doNotUseCurrentStatement;

            set
            {
                if (Equals(value, this.doNotUseCurrentStatement))
                {
                    return;
                }

                this.doNotUseCurrentStatement = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoStatementLoaded));
                OnPropertyChanged(nameof(AddNewReconciliationIsEnabled));
            }
        }

        /// <summary>
        ///     This variable is used to contain the newly added ledger line when doing a new reconciliation. When this is non-null it also indicates the ledger row can be edited.
        /// </summary>
        public LedgerEntryLine? NewLedgerLine
        {
            get => this.doNotUseNewLedgerLine;
            set
            {
                if (Equals(value, this.doNotUseNewLedgerLine))
                {
                    return;
                }

                this.doNotUseNewLedgerLine = value;
                OnPropertyChanged();
            }
        }
    }
}
