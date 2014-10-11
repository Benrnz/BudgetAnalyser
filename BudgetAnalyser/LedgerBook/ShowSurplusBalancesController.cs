using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ShowSurplusBalancesController : ControllerBase
    {
        private LedgerEntryLine ledgerEntryLine;

        public decimal BankBalanceTotal
        {
            get { return this.ledgerEntryLine.CalculatedSurplus; }
        }

        public ObservableCollection<BankBalance> BankBalances { get; private set; }

        public bool CanExecuteCancelButton
        {
            get { return false; }
        }

        public bool CanExecuteOkButton
        {
            get { return true; }
        }

        public ICommand RemoveBankBalanceCommand
        {
            get
            {
                // This is here solely to disable the Remove Bank Balance button on the default DataTemplate that displays the BankBalance type.
                return new RelayCommand<BankBalance>(b => { }, b => false);
            }
        }

        public bool CanExecuteSaveButton
        {
            get { return false; }
        }

        public void ShowDialog(LedgerEntryLine ledgerLine)
        {
            BankBalances = new ObservableCollection<BankBalance>(ledgerLine.SurplusBalances);
            this.ledgerEntryLine = ledgerLine;

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.Ok)
            {
                CorrelationId = Guid.NewGuid(),
                Title = "Surplus Balances in all Accounts",
            };

            MessengerInstance.Send(dialogRequest);
        }
    }
}