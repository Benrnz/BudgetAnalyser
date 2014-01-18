using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Statement
{
    public class StatementHasBeenModifiedMessage : MessageBase
    {
        public bool Dirty { get; set; }
    }
}