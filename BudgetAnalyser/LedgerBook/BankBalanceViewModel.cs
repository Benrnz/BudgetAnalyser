using System;
using System.Linq;
using BudgetAnalyser.Annotations;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    /// A view model that wraps the <see cref="BankBalance"/> class to include a adjusted balance.
    /// </summary>
    public class BankBalanceViewModel : BankBalance
    {
        private readonly LedgerEntryLine line;

        public BankBalanceViewModel([CanBeNull] LedgerEntryLine line, [NotNull] BankBalance balance) : base(balance.Account, balance.Balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException("balance");
            }

            this.line = line;
        }

        public BankBalanceViewModel([CanBeNull] LedgerEntryLine line, [NotNull] Account account, decimal balance) : base(account, balance)
        {
            this.line = line;
        }

        public decimal AdjustedBalance
        {
            get
            {
                if (this.line == null) return Balance;
                var adjustment = this.line.BankBalanceAdjustments.FirstOrDefault(b => b.BankAccount == Account);
                return adjustment == null ? Balance : Balance + adjustment.Amount;
            }
        }
    }
}
