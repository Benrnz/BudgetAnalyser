using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Rees.Wpf.Contracts;

namespace Rees.Wpf
{
    /// <summary>
    ///     Used to abstract away from instantiating and interacting with real windows within Controller and ViewModel code.
    /// </summary>
    /// <typeparam name="T">The view load.</typeparam>
    public class WpfViewLoader<T> : IViewLoader where T : Window, new()
    {
        public double? Height { get; set; }
        public double? MinHeight { get; set; }
        public double? MinWidth { get; set; }
        protected T TargetWindow { get; set; }
        public double? Width { get; set; }

        /// <summary>
        ///     Close and unload the view immediately.
        /// </summary>
        public virtual void Close()
        {
            if (TargetWindow is not null)
            {
                TargetWindow.Close();
                TargetWindow = null;
            }
        }

        /// <summary>
        ///     Show the view in a normal way.
        /// </summary>
        /// <param name="context">The model or context the view can use for binding or reference purposes.</param>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
                         Justification = "Clean up code. The window is orphaned or has been instructed to close.")]
        public virtual void Show(object context)
        {
            if (TargetWindow is not null)
            {
                try
                {
                    TargetWindow.Close();
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                    // Swallow any exception trying to close the orphaned window.
                }
            }

            TargetWindow = CreateWindow();
            ConfigureWindow(context);
            TargetWindow.Show();
        }

        /// <summary>
        ///     Show the view in a dialog manner.
        /// </summary>
        /// <param name="context">The model or context the view can use for binding or reference purposes.</param>
        /// <returns>
        ///     Used to indicate how the user closed the dialog.  Can be used to determine the difference between cancelling
        ///     or cofirmation.
        /// </returns>
        public virtual bool? ShowDialog(object context)
        {
            TargetWindow = new T { DataContext = context, Owner = Application.Current.MainWindow };
            var result = TargetWindow.ShowDialog();
            TargetWindow = null;
            return result;
        }

        /// <summary>
        ///     Configures the window with the given height, width parameters etc.
        /// </summary>
        /// <param name="context">The Data Context.</param>
        protected virtual void ConfigureWindow(object context)
        {
            TargetWindow.DataContext = context;
            if (Height is not null)
            {
                TargetWindow.Height = Height.Value;
            }

            if (Width is not null)
            {
                TargetWindow.Width = Width.Value;
            }

            if (MinHeight is not null)
            {
                TargetWindow.MinHeight = MinHeight.Value;
            }

            if (MinWidth is not null)
            {
                TargetWindow.MinWidth = MinWidth.Value;
            }
        }

        /// <summary>
        ///     Creates the window.
        /// </summary>
        protected virtual T CreateWindow()
        {
            return new T();
        }
    }
}
