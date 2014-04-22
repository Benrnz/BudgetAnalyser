using System;

namespace BudgetAnalyser.Engine
{
    public interface IApplicationHookEventPublisher
    {
        event EventHandler<ApplicationHookEventArgs> ApplicationEvent;
    }
}