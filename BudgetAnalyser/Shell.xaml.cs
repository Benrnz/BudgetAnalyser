using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow
    {
        private bool closeHandled;
        private bool sizeHasBeenSet;

        public ShellWindow()
        {
            InitializeComponent();
        }

        private ShellController Controller
        {
            get { return (ShellController)DataContext; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Controller.OnViewReady();

            // Data binding these properties doesnt seem to work so well. Desired values are overwritten with other values multiple times.
            Width = Controller.WindowSize.X;
            Height = Controller.WindowSize.Y;
            Left = Controller.WindowTopLeft.X;
            Top = Controller.WindowTopLeft.Y;

            // This is necessary because the controller must not be notified until the above sizes have been set, this doesnt happen immediately otherwise the desired value is overwritten.
            this.sizeHasBeenSet = true;
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            if (this.sizeHasBeenSet)
            {
                Controller.NotifyOfWindowLocationChange(new Point(Left, Top));
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.sizeHasBeenSet)
            {
                Controller.NotifyOfWindowSizeChange(new Point(ActualWidth, ActualHeight));
            }
        }

        private async void ShellWindow_OnClosing(object sender, CancelEventArgs e)
        {
            // While the application is closing using async tasks and the task factory is error prone.
            // A better approach is to cancel the close, handle the save then re-trigger close.
            if (!this.closeHandled)
            {
                this.closeHandled = true;
                e.Cancel = true;
                await Controller.ShellClosing();
                try
                {
                    Close();
                }
                catch
                {
                    // Swallow - this may throw if the application is already closing.
                }
            }
        }
    }
}