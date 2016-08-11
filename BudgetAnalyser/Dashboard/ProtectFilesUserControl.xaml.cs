using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace BudgetAnalyser.Dashboard
{
    /// <summary>
    ///     Interaction logic for ProtectFilesUserControl.xaml
    /// </summary>
    public partial class ProtectFilesUserControl
    {
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
            SendPasswordToController();
        }

        private void OnPasswordLostFocus(object sender, RoutedEventArgs e)
        {
            SendPasswordToController();
        }

        private void OnWindowIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                this.controller = (EncryptFileController)DataContext;
            }
            if ((bool)e.NewValue == false && (bool)e.OldValue == true)
            {
                this.passwordBox.Clear();
                this.confirmBox.Clear();
            }
        }

        private void SendPasswordToController()
        {
            this.controller.SetPassword(this.passwordBox.SecurePassword);
        }
    }
}