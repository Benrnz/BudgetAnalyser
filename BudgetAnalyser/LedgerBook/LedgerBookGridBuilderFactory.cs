using BudgetAnalyser.Engine;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC]
public class LedgerBookGridBuilderFactory
{
    public virtual ILedgerBookGridBuilder GridBuilderV2(TabLedgerBookController tabLedgerBookController)
    {
        return tabLedgerBookController is null
            ? throw new ArgumentNullException(nameof(tabLedgerBookController))
            : (ILedgerBookGridBuilder)new LedgerBookGridBuilderV2(
                tabLedgerBookController.ShowTransactionsCommand,
                tabLedgerBookController.ShowBankBalancesCommand,
                tabLedgerBookController.ShowRemarksCommand,
                tabLedgerBookController.ShowHidePeriodsCommand,
                tabLedgerBookController.ShowSurplusBalancesCommand,
                tabLedgerBookController.ShowLedgerBucketDetailsCommand);
    }
}
