using BudgetAnalyser.Engine.Ledger;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook;

public class LedgerBucketTransferCommandMessage(TransferFundsCommand transferFundsCommand) : MessageBase
{
    public TransferFundsCommand TransferFundsCommand { get; private set; } = transferFundsCommand;
}
