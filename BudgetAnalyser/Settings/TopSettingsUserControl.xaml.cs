using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.Settings;

/// <summary>
///     Interaction logic for TopSettingsUserControl.xaml
/// </summary>
public partial class TopSettingsUserControl : UserControl
{
    public TopSettingsUserControl()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     WPF cannot data bind <see cref="PasswordBox.Password" />, so secret bank feed credentials are passed to their row here. The value is never written back to the box.
    /// </summary>
    private void OnBankFeedSecretChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox { DataContext: BankFeedCredentialRowViewModel row } passwordBox)
        {
            row.NewValue = passwordBox.Password;
        }
    }
}
