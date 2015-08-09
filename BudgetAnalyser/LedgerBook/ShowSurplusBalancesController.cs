using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.ShellDialog;
using GalaSoft.MvvmLight.CommandWpf;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ShowSurplusBalancesController : ControllerBase
    {
        private LedgerEntryLine ledgerEntryLine;

        public  bool HasNegativeBalances => SurplusBalances.Any(b => b.Balance < 0);

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Instance method required for data binding")]
        public ICommand RemoveBankBalanceCommand
        {
            get
            {
                // This is here solely to disable the Remove Bank Balance button on the default DataTemplate that displays the BankBalance type.
                return new RelayCommand<BankBalance>(b => { }, b => false);
            }
        }

        public ObservableCollection<BankBalance> SurplusBalances { get; private set; }

        public  decimal SurplusTotal => this.ledgerEntryLine.CalculatedSurplus;

        public void ShowDialog([NotNull] LedgerEntryLine ledgerLine)
        {
            if (ledgerLine == null)
            {
                throw new ArgumentNullException(nameof(ledgerLine));
            }

            SurplusBalances = new ObservableCollection<BankBalance>(ledgerLine.SurplusBalances);
            this.ledgerEntryLine = ledgerLine;

            var dialogRequest = new ShellDialogRequestMessage(BudgetAnalyserFeature.LedgerBook, this, ShellDialogType.Ok)
            {
                CorrelationId = Guid.NewGuid(),
                Title = "Surplus Balances in all Accounts"
            };

            MessengerInstance.Send(dialogRequest);
        }
    }
}