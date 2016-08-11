using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private EncryptFileController controller;

        public ProtectFilesUserControl()
        {
            InitializeComponent();
        }

        private void OnConfirmBoxKeyUp(object sender, KeyEventArgs e)
        {
            this.controller.SetConfirmedPassword(this.passwordBox.Password == this.confirmBox.Password);
        }

        private void OnPasswordKeyUp(object sender, KeyEventArgs e)
        {
            if (this.finishedEnteringTask != null)
            {
                // If 1 second is not up yet and another keystroke is received, reset the timer.
                this.cancellationSource.Cancel();
                this.cancellationSource.Dispose();
            }

            this.cancellationSource = new CancellationTokenSource();
            var delay = this.passwordBox.Password.Length > 4 ? TimeSpan.FromMilliseconds(300) : 1.Seconds();
            this.finishedEnteringTask = Task.Delay(delay, this.cancellationSource.Token)
                .ContinueWith(t =>
                {
                    if (t.IsCanceled) return;
                    DispatchPasswordToController();
                });
        }

        private void OnPasswordLostFocus(object sender, RoutedEventArgs e)
        {
            SendPasswordToController();
            e.Handled = true;
        }

        private void OnWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                this.controller = (EncryptFileController) DataContext;
            }
            if ((bool)e.NewValue == false && (bool)e.OldValue == true)
            {
                this.passwordBox.Clear();
                this.confirmBox.Clear();
                this.cancellationSource?.Cancel();
                this.cancellationSource?.Dispose();
            }
        }

        private void DispatchPasswordToController()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, SendPasswordToController);
        }

        private void SendPasswordToController()
        {
            if (this.passwordBox.SecurePassword.Length > 0)
            {
                this.controller.SetPassword(this.passwordBox.SecurePassword);
                Debug.WriteLine($"{DateTime.Now} Sent password to controller {this.passwordBox.Password }");
            }
        }
    }
}