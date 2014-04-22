namespace BudgetAnalyser.Engine
{
    public interface IApplicationHookSubscriber
    {
        void OnEventOccured(object sender, ApplicationHookEventArgs args);
    }
}