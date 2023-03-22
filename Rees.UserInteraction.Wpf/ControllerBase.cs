using System.Windows.Threading;
using GalaSoft.MvvmLight;

namespace Rees.Wpf
{
    /// <summary>
    /// Extension to the MvvmLight <see cref="ViewModelBase"/> to include a reference to the Dispatcher for the thread that
    /// executes the constructor. 
    /// </summary>
    public class ControllerBase : ViewModelBase
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // Required for testing
        private readonly Dispatcher doNotUseDispatcher;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerBase"/> class.
        /// </summary>
        public ControllerBase()
        {
            // This relies on the Xaml being responsible for instantiating the controller.
            // Or at least the main UI thread.
            this.doNotUseDispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Gets the dispatcher that was stored by the constructor. This will be the main UI thread dispatcher.
        /// </summary>
        protected Dispatcher Dispatcher
        {
            get { return this.doNotUseDispatcher; }
        }
    }
}