using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     A view model that wraps the <see cref="BankBalance" /> class to include a adjusted balance.
    /// </summary>
    public class BankBalanceViewModel : BankBalance
    {
        private readonly LedgerEntryLine line;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "It is validated.")]
        public BankBalanceViewModel([CanBeNull] LedgerEntryLine line, [NotNull] BankBalance balance) : base(balance.Account, balance.Balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
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
                if (this.line == null)
                {
                    return Balance;
                }
                var adjustment = this.line.BankBalanceAdjustments.Where(b => b.BankAccount == Account).Sum(b => b.Amount);
                return Balance + adjustment;
            }
        }

        [UsedImplicitly]
        public bool ShowAdjustedBalance => this.line is not null;
    }
}