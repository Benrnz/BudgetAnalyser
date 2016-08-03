using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
    /// <summary>
    ///     Interaction logic for ProtectFilesUserControl.xaml
    /// </summary>
    public partial class ProtectFilesUserControl
    {
        private CancellationTokenSource cancellationSource;
        private Task finishedEnteringTask;

        public ProtectFilesUserControl()
        {
            InitializeComponent();
        }

        private EncryptFileController Controller => (EncryptFileController) DataContext;

        private void OnPasswordKeyUp(object sender, KeyEventArgs e)
        {
            if (this.finishedEnteringTask != null)
            {
                // If 1 second is not up yet and another keystroke is received, reset the timer.
                this.cancellationSource.Cancel();
                this.cancellationSource.Dispose();
            }

            this.cancellationSource = new CancellationTokenSource();
            this.finishedEnteringTask = Task.Delay(1.Seconds(), this.cancellationSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    SendPasswordToController();
                });
        } 

        private void OnPasswordLostFocus(object sender, RoutedEventArgs e)
        {
            SendPasswordToController();
        }

        private void OnWindowLostFocus(object sender, RoutedEventArgs e)
        {
            if (!(sender is ProtectFilesUserControl)) return;
            
            this.passwordBox.Clear();
            this.confirmBox.Clear();
            this.cancellationSource?.Cancel();
            this.cancellationSource?.Dispose();
        }

        private void SendPasswordToController()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                if (Controller == null) return;
                if (this.passwordBox.SecurePassword.Length > 0)
                {
                    Controller.SetPassword(this.passwordBox.SecurePassword);
                    Debug.WriteLine($"{DateTime.Now} Sent password to controller");
                }
            });
        }

        private void OnConfirmBoxKeyUp(object sender, KeyEventArgs e)
        {
            Controller.SetConfirmedPassword(this.passwordBox.Password == this.confirmBox.Password);
        }

        private void OnWindowGotFocus(object sender, RoutedEventArgs e)
        {
        }
    }
}