using System;
using BudgetAnalyser.Matching;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser
{
    /// <summary>
    /// An interface for all things UIContext.
    /// </summary>
    public interface IUiContext
    {
        // TODO More to add here when required.
        AppliedRulesController AppliedRulesController { get; set; }
        IBackgroundProcessingJobMetadata BackgroundJob { get; }
        IMessenger Messenger { get; }
        UserPrompts UserPrompts { get; }
        Func<IWaitCursor> WaitCursorFactory { get; }
    }
}