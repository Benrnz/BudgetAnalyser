namespace BudgetAnalyser.Engine.Services
{
    public class AdditionalInformationRequestedEventArgs : ValidatingEventArgs
    {
        public string ModificationComment { get; set; }
    }
}