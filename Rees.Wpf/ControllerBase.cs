using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Rees.Wpf
{
    /// <summary>
    ///     Extension to the MvvmLight <see cref="ViewModelBase" /> to include a reference to the Dispatcher for the thread
    ///     that
    ///     executes the constructor.
    /// </summary>
    public abstract class ControllerBase : ObservableRecipient
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ControllerBase" /> class.
        /// </summary>
        public ControllerBase(IMessenger messenger) : base(messenger)
        {
            // This relies on the Xaml being responsible for instantiating the controller.
            // Or at least the main UI thread.
            Dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        ///     Gets the dispatcher that was stored by the constructor. This will be the main UI thread dispatcher.
        /// </summary>
        protected Dispatcher Dispatcher { get; }
    }
}