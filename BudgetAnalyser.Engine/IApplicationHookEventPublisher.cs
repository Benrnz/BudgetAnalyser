using System;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    /// An interface to describe an event source (publisher) for application events.
    /// This can be any class to which wishes to broadcast an application-wide event for events such as
    /// start-up, errors, shutdown, file-load, file-save, validation warnings etc.
    /// </summary>
    public interface IApplicationHookEventPublisher
    {
        event EventHandler<ApplicationHookEventArgs> ApplicationEvent;
    }
}