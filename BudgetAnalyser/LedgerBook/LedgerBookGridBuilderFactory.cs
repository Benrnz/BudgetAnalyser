using BudgetAnalyser.Engine;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public class LedgerBookGridBuilderFactory
    {
        public virtual ILedgerBookGridBuilder GridBuilderV2(LedgerBookController ledgerBookController)
        {
            return new LedgerBookGridBuilderV2(
                ledgerBookController.ShowTransactionsCommand,
                ledgerBookController.ShowBankBalancesCommand,
                ledgerBookController.ShowRemarksCommand,
                ledgerBookController.RemoveLedgerEntryLineCommand,
                ledgerBookController.ShowHideMonthsCommand,
                ledgerBookController.ShowSurplusBalancesCommand,
                ledgerBookController.ShowLedgerColumnDetailsCommand);
        }
    }
}