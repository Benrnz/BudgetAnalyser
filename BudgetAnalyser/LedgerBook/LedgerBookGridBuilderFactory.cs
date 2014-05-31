using System.Windows.Input;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC]
    public class LedgerBookGridBuilderFactory
    {
        public virtual ILedgerBookGridBuilder GridBuilderV1(
            ICommand showTransactionsCommand,
            ICommand showBankBalancesCommand,
            ICommand showRemarksCommand,
            ICommand removeLedgerEntryLineCommand)
        {
            return new LedgerBookGridBuilder(showTransactionsCommand, showBankBalancesCommand, showRemarksCommand, removeLedgerEntryLineCommand);
        }

        //public virtual ILedgerBookGridBuilder GridBuilderV2(
        //    ICommand showTransactionsCommand,
        //    ICommand showBankBalancesCommand,
        //    ICommand showRemarksCommand,
        //    ICommand removeLedgerEntryLineCommand)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
