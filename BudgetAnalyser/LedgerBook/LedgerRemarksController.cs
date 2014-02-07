using System;
using System.Windows.Input;
using BudgetAnalyser.Engine.Ledger;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerRemarksController : ControllerBase, IShowableController
    {
        private bool doNotUseIsReadOnly;
        private LedgerEntryLine doNotUseLedgerEntryLine;
        private string doNotUseRemarks;
        private bool doNotUseShown;

        public event EventHandler Completed;

        public ICommand CloseCommand
        {
            get { return new RelayCommand(OnCloseCommandExecuted); }
        }

        public bool IsReadOnly
        {
            get { return this.doNotUseIsReadOnly; }
            private set
            {
                this.doNotUseIsReadOnly = value;
                RaisePropertyChanged(() => IsReadOnly);
            }
        }

        public LedgerEntryLine LedgerEntryLine
        {
            get { return this.doNotUseLedgerEntryLine; }
            private set
            {
                this.doNotUseLedgerEntryLine = value;
                RaisePropertyChanged(() => LedgerEntryLine);
            }
        }

        public string Remarks
        {
            get { return this.doNotUseRemarks; }
            set
            {
                this.doNotUseRemarks = value;
                RaisePropertyChanged(() => Remarks);
            }
        }

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }

        public void Show(LedgerEntryLine line, bool isNew)
        {
            LedgerEntryLine = line;
            Remarks = LedgerEntryLine.Remarks;
            IsReadOnly = !isNew;
            Shown = true;
        }

        private void OnCloseCommandExecuted()
        {
            if (!IsReadOnly)
            {
                LedgerEntryLine.UpdateRemarks(Remarks);
            }

            LedgerEntryLine = null;
            Remarks = null;
            Shown = false;

            EventHandler handler = Completed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}