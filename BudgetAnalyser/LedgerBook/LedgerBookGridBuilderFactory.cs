using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Annotations;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public class LedgerBookGridBuilderFactory
    {
        public virtual ILedgerBookGridBuilder GridBuilderV2([NotNull] LedgerBookController ledgerBookController)
        {
            if (ledgerBookController == null)
            {
                throw new ArgumentNullException(nameof(ledgerBookController));
            }

            return new LedgerBookGridBuilderV2(
                ledgerBookController.ShowTransactionsCommand,
                ledgerBookController.ShowBankBalancesCommand,
                ledgerBookController.ShowRemarksCommand,
                ledgerBookController.ShowHidePeriodsCommand,
                ledgerBookController.ShowSurplusBalancesCommand,
                ledgerBookController.ShowLedgerBucketDetailsCommand);
        }
    }
}