using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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
    ///     Opens the bank feed provider's setup instructions in the default browser. Only https addresses are opened.
    /// </summary>
    private void OnBankFeedSetupInstructionsRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Handled = true;
        if (e.Uri is not { IsAbsoluteUri: true } uri || uri.Scheme != Uri.UriSchemeHttps)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
        catch (Win32Exception)
        {
            // No browser is available to open the link. The address is shown in the link text, so the user can still copy it.
        }
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
