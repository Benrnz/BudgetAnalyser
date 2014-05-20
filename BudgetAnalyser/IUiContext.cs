using System;
using BudgetAnalyser.Matching;
using BudgetAnalyser.Statement;
using GalaSoft.MvvmLight.Messaging;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser
{
    /// <summary>
    ///     An interface for all things UIContext.
    /// </summary>
    public interface IUiContext
    {
        // TODO More to add here when required.
        AppliedRulesController AppliedRulesController { get; set; }
        IBackgroundProcessingJobMetadata BackgroundJob { get; }
        EditingTransactionController EditingTransactionController { get; }
        IMessenger Messenger { get; }
        SplitTransactionController SplitTransactionController { get; }
        UserPrompts UserPrompts { get; }
        Func<IWaitCursor> WaitCursorFactory { get; }
    }
}