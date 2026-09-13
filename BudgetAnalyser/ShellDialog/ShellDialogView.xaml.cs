using System.Windows.Controls;
using System.Windows.Input;

namespace BudgetAnalyser.ShellDialog;

/// <summary>
///     Interaction logic for ShellDialogView.xaml
/// </summary>
public partial class ShellDialogView : UserControl
{
    public ShellDialogView()
    {
        InitializeComponent();
    }

    private ShellDialogController Controller => (ShellDialogController)DataContext;

    private void OnShellDialogKeyUp(object? sender, KeyEventArgs e)
    {
        if (Controller.Content is null)
        {
            return;
        }

        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0 && (Keyboard.Modifiers & ModifierKeys.Alt) == 0)
            {
                // Only if Shift, Ctrl or Alt is not pressed.
                Controller.DialogCommand.Execute(ShellDialogButton.Ok);
            }
        }
        else if (e.Key == Key.Escape)
        {
            Controller.DialogCommand.Execute(ShellDialogButton.Cancel);
        }

        e.Handled = true;
    }
}
