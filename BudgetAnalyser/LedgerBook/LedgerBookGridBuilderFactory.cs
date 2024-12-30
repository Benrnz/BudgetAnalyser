using BudgetAnalyser.Engine;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public class LedgerBookGridBuilderFactory
    {
        public virtual ILedgerBookGridBuilder GridBuilderV2([NotNull] LedgerBookController ledgerBookController)
        {
            return ledgerBookController is null
                ? throw new ArgumentNullException(nameof(ledgerBookController))
                : (ILedgerBookGridBuilder)new LedgerBookGridBuilderV2(
                ledgerBookController.ShowTransactionsCommand,
                ledgerBookController.ShowBankBalancesCommand,
                ledgerBookController.ShowRemarksCommand,
                ledgerBookController.ShowHidePeriodsCommand,
                ledgerBookController.ShowSurplusBalancesCommand,
                ledgerBookController.ShowLedgerBucketDetailsCommand);
        }
    }
}
