using System;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class AddLedgerReconciliationController : ControllerBase, IShowableController
    {
        private decimal doNotUseBankBalance;
        private DateTime doNotUseDate;
        private bool doNotUseShown;

        public event EventHandler Complete;

        public decimal BankBalance
        {
            get { return this.doNotUseBankBalance; }
            set
            {
                this.doNotUseBankBalance = value;
                RaisePropertyChanged(() => BankBalance);
            }
        }

        public ICommand CancelCommand
        {
            get { return new RelayCommand(OnCancelCommandExecuted); }
        }

        public bool Cancelled { get; private set; }

        public DateTime Date
        {
            get { return this.doNotUseDate; }
            set
            {
                this.doNotUseDate = value;
                RaisePropertyChanged(() => Date);
            }
        }

        public ICommand OkCommand
        {
            get { return new RelayCommand(OnOkCommandExecuted); }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                if (value)
                {
                    Date = DateTime.Today;
                    Cancelled = false;
                }

                RaisePropertyChanged(() => Shown);
            }
        }

        private void Close()
        {
            Shown = false;
            EventHandler handler = Complete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnCancelCommandExecuted()
        {
            Cancelled = true;
            Close();
        }

        private void OnOkCommandExecuted()
        {
            Cancelled = false;
            Close();
        }
    }
}