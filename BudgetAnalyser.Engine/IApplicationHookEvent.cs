using System;

namespace BudgetAnalyser.Engine
{
    public interface IApplicationHookEvent
    {
        event EventHandler<ApplicationHookEventArgs> ApplicationEvent;
    }
}