using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.Dashboard
{
    public class PasswordSetMessage : MessageBase
    {
        public string DatabaseStorageKey { get; set; }
    }
}