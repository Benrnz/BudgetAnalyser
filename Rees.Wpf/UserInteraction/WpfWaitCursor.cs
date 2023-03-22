using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using System.Windows.Threading;
using Rees.Wpf.Contracts;

namespace Rees.Wpf.UserInteraction
{
    /// <summary>
    ///     A Wpf implementation of a wait cursor. Intended to be used in a using block.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
                     Justification = "Used for coding style only.")]
    public sealed class WpfWaitCursor : IWaitCursor
    {
        private readonly Dispatcher dispatcher;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WpfWaitCursor" /> class.
        /// </summary>
        public WpfWaitCursor()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            this.dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
                         Justification = "Used for coding style only.")]
        [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly",
                         Justification = "Used for coding style only.")]
        public void Dispose()
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                                        new Action(() => Mouse.OverrideCursor = null));
        }
    }
}