namespace BudgetAnalyser.Engine
{
    public interface IApplicationHookSubscriber
    {
        void OnEventOccurred(object sender, ApplicationHookEventArgs args);
    }
}