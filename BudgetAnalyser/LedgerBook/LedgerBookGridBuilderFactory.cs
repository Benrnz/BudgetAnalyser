using BudgetAnalyser.Engine;

namespace BudgetAnalyser.LedgerBook;

[AutoRegisterWithIoC]
public class LedgerBookGridBuilderFactory(ILogger logger)
{

    public virtual ILedgerBookGridBuilder GridBuilderV2(TopLedgerBookController ledgerBookController)
    {
        return ledgerBookController is null
            ? throw new ArgumentNullException(nameof(ledgerBookController))
            : (ILedgerBookGridBuilder)new LedgerBookGridBuilderV2(
                logger,
                ledgerBookController.ShowTransactionsCommand,
                ledgerBookController.ShowBankBalancesCommand,
                ledgerBookController.ShowRemarksCommand,
                ledgerBookController.ShowHidePeriodsCommand,
                ledgerBookController.ShowSurplusBalancesCommand,
                ledgerBookController.ShowLedgerBucketDetailsCommand);
    }
}
