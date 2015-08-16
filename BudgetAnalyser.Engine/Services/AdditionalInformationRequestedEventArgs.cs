namespace BudgetAnalyser.Engine.Services
{
    public class AdditionalInformationRequestedEventArgs : ValidatingEventArgs
    {
        public object Context { get; set; }
        public string ModificationComment { get; set; }
    }
}