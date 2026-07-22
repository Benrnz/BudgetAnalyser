using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Rees.Wpf;

/// <summary>
///     Extension to the Community Toolkit <see cref="ObservableRecipient" /> to include a reference to the Dispatcher for the thread that executes the constructor.
/// </summary>
public abstract class ControllerBase : ObservableRecipient
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ControllerBase" /> class.
    /// </summary>
    protected ControllerBase(IMessenger messenger) : base(messenger)
    {
        // This relies on the Xaml being responsible for instantiating the controller. Or at least the main UI thread.
        Dispatcher = Dispatcher.CurrentDispatcher;
    }

    /// <summary>
    ///     Gets the dispatcher that was stored by the constructor. This will be the main UI thread dispatcher.
    /// </summary>
    protected Dispatcher Dispatcher { get; }

    /// <summary>
    ///     For use with async relay command handlers to perform some logging action if the handler throws an error.
    /// </summary>
    /// <param name="task">The task to observe for unhandled exceptions.</param>
    /// <param name="loggingAction">The action to perform if an exception is thrown.</param>
    protected void ObserveUnhandledFireAndForgetFailure(Task task, Action<Exception> loggingAction)
    {
        _ = task.ContinueWith(
            t =>
            {
                var baseException = t.Exception?.GetBaseException();
                if (baseException is not null)
                {
                    loggingAction(baseException);
                }
            },
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
    }
}
