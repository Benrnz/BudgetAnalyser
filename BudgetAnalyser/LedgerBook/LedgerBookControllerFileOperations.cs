using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Services;
using GalaSoft.MvvmLight.Messaging;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookControllerFileOperations : INotifyPropertyChanged
    {
        private readonly IApplicationDatabaseService applicationDatabaseService;
        private bool doNotUseDirty;

        public LedgerBookControllerFileOperations(
            [NotNull] IMessenger messenger,
            [NotNull] IApplicationDatabaseService applicationDatabaseService)
        {
            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException(nameof(applicationDatabaseService));
            }

            this.applicationDatabaseService = applicationDatabaseService;
            MessengerInstance = messenger;

            ViewModel = new LedgerBookViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public IMessenger MessengerInstance { get; }

        internal bool Dirty
        {
            get { return this.doNotUseDirty; }
            set
            {
                this.doNotUseDirty = value;
                OnPropertyChanged();
                if (Dirty)
                {
                    this.applicationDatabaseService.NotifyOfChange(ApplicationDataType.Ledger);
                }
            }
        }

        /// <summary>
        ///     Gets or sets the ledger service. Will be set by the <see cref="LedgerBookController" /> during its initialisation.
        /// </summary>
        internal ILedgerService LedgerService { get; set; }

        internal LedgerBookViewModel ViewModel { get; }

        public void Close()
        {
            ViewModel.LedgerBook = null;
            MessengerInstance.Send(new LedgerBookReadyMessage(null));
        }

        internal void SyncDataFromLedgerService()
        {
            ViewModel.LedgerBook = LedgerService.LedgerBook;
            MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook) { ForceUiRefresh = true });
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}