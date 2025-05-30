﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BudgetAnalyser.LedgerBook;

/// <summary>
///     Interaction logic for AddLedgerReconciliationUserControl.xaml
/// </summary>
public partial class AddLedgerReconciliationUserControl : UserControl
{
    public AddLedgerReconciliationUserControl()
    {
        InitializeComponent();
    }

    private void OnAddBankBalanceClick(object? sender, RoutedEventArgs e)
    {
        this.BankBalance.Focus();
        this.BankBalance.SelectAll();
    }

    private void OnBankBalanceGotFocus(object? sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void OnBankBalanceKeyDown(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox balanceBox)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Decimal:
            case Key.OemPeriod:
                // Don't allow 2 decimal places.
                var index = balanceBox.Text.IndexOf('.');
                if (index >= 0)
                {
                    balanceBox.CaretIndex = index + 1 >= balanceBox.Text.Length ? balanceBox.Text.Length - 1 : index + 1;
                    e.Handled = true;
                }

                break;
        }
    }

    private void OnBankBalanceKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not TextBox balanceBox)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Back:
                // Work around to fix the weird behaviour of using a number format in the binding. Backspacing over a decimal multiplies the number by 100?!
                // Cannot detect Backspace on Key Down
                var chars = balanceBox.Text.ToCharArray();
                if (chars[balanceBox.CaretIndex - 1] == '.')
                {
                    balanceBox.CaretIndex = balanceBox.CaretIndex - 1;
                    e.Handled = true;
                }

                break;
        }
    }

    private void OnBankBalanceMouseUp(object? sender, MouseButtonEventArgs e)
    {
        this.BankBalance.SelectAll();
    }

    private void OnIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool visible)
        {
            if (visible)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => this.BankBalance.Focus());
            }
        }
    }
}
