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
                throw new ArgumentNullException("messenger");
            }

            if (applicationDatabaseService == null)
            {
                throw new ArgumentNullException("applicationDatabaseService");
            }

            this.applicationDatabaseService = applicationDatabaseService;
            MessengerInstance = messenger;

            ViewModel = new LedgerBookViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        public IMessenger MessengerInstance { get; set; }

        internal LedgerBookViewModel ViewModel { get; set; }

        public void Close()
        {
            ViewModel.LedgerBook = null;
            MessengerInstance.Send(new LedgerBookReadyMessage(null));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void SyncDataFromLedgerService()
        {
            ViewModel.LedgerBook = LedgerService.LedgerBook;
            MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook) { ForceUiRefresh = true });
        }

        //private bool CanExecuteNewLedgerBookCommand()
        //{
        //    // TODO Temporarily disabled while introducing ApplicationDatabaseService
        //    return ViewModel.LedgerBook == null;
        //}

        //private void OnNewLedgerBookCommandExecuted()
        //{
        //    // TODO Temporarily disabled while introducing ApplicationDatabaseService
        //    IUserPromptSaveFile saveFileDialog = this.saveFileDialogFactory();
        //    saveFileDialog.AddExtension = true;
        //    saveFileDialog.CheckPathExists = true;
        //    saveFileDialog.DefaultExt = ".xml";
        //    saveFileDialog.Filter = "LedgerBook files (*.xml, *.xaml)|*.xml;*.xaml|All files (*.*)|*.*";
        //    saveFileDialog.Title = "Choose a LedgerBook xml file name.";
        //    bool? result = saveFileDialog.ShowDialog();
        //    if (result == null || !result.Value)
        //    {
        //        return;
        //    }

        //    string fileName = saveFileDialog.FileName;
        //    if (string.IsNullOrWhiteSpace(fileName))
        //    {
        //        return;
        //    }

        //    Close();

        //    ViewModel.LedgerBook = LedgerService.CreateNew(fileName);
        //    Dirty = true;
        //    MessengerInstance.Send(new LedgerBookReadyMessage(ViewModel.LedgerBook));
        //}
    }
}