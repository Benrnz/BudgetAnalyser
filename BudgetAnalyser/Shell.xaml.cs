using System.ComponentModel;
using System.Windows;

namespace BudgetAnalyser;

/// <summary>
///     Interaction logic for ShellWindow.xaml
/// </summary>
public partial class ShellWindow
{
    private bool sizeHasBeenSet;
    private bool isClosingAfterSave;

    public ShellWindow()
    {
        InitializeComponent();
    }

    private ShellController Controller => (ShellController)DataContext;

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Data binding these properties doesn't seem to work so well. Desired values are overwritten with other values multiple times.
        if (Controller.WindowSize.X == 0 || Controller.WindowSize.Y == 0)
        {
            Controller.WindowSize = new Point(Width, Height);
            Controller.WindowTopLeft = new Point(Left, Top);
        }

        Width = Controller.WindowSize.X;
        Height = Controller.WindowSize.Y;
        Left = Controller.WindowTopLeft.X;
        Top = Controller.WindowTopLeft.Y;

        // This is necessary because the controller must not be notified until the above sizes have been set, this doesnt happen immediately otherwise the desired value is overwritten.
        this.sizeHasBeenSet = true;
    }

    private void OnLocationChanged(object? sender, EventArgs e)
    {
        if (this.sizeHasBeenSet)
        {
            Controller.NotifyOfWindowLocationChange(new Point(Left, Top));
        }
    }

    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (this.sizeHasBeenSet)
        {
            Controller.NotifyOfWindowSizeChange(new Point(ActualWidth, ActualHeight));
        }
    }

    private async void ShellWindow_OnClosing(object? sender, CancelEventArgs e)
    {
        if (this.isClosingAfterSave)
        {
            this.isClosingAfterSave = false; // allow the second close to complete
            return;
        }

        Controller.SaveApplicationState();

        if (!Controller.HasUnsavedChanges)
        {
            return;
        }

        e.Cancel = true; // keep app alive while async close/save work finishes
        await Controller.ShellClosing();
        this.isClosingAfterSave = true;
        Close(); // triggers Closing again, now allowed through
    }
}
