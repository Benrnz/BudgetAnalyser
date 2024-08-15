using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.ShellDialog
{
    public class ShellDialogController : ControllerBase
    {
        private bool doNotUseCancelButtonVisible;
        private object doNotUseContent;
        private ShellDialogType doNotUseDialogType;
        private bool doNotUseHelpButtonVisible;
        private bool doNotUseOkButtonVisible;
        private bool doNotUseSaveButtonVisible;
        private string doNotUseTitle;
        private RelayCommand<ShellDialogButton> dialogRelayCommand;

        public ShellDialogController([NotNull] IMessenger messenger) : base(messenger)
        {
            DialogType = ShellDialogType.OkCancel;
            this.dialogRelayCommand = new RelayCommand<ShellDialogButton>(OnDialogCommandExecute, CanExecuteDialogCommand);
            DialogCommand = this.dialogRelayCommand;
            messenger.Register<ShellDialogCommandRequerySuggestedMessage>(this, (l, m) => this.dialogRelayCommand.NotifyCanExecuteChanged());
        }

        public string ActionToolTip
        {
            get
            {
                var customTooltips = Content as IShellDialogToolTips;
                if (customTooltips == null)
                {
                    return DialogType == ShellDialogType.SaveCancel ? "Save" : "Ok";
                }

                return customTooltips.ActionButtonToolTip;
            }
        }

        public bool CancelButtonVisible
        {
            get { return this.doNotUseCancelButtonVisible; }
            set
            {
                this.doNotUseCancelButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public string CloseToolTip
        {
            get
            {
                var customTooltips = Content as IShellDialogToolTips;
                if (customTooltips == null)
                {
                    return "Close";
                }

                return customTooltips.CloseButtonToolTip;
            }
        }

        public object Content
        {
            get { return this.doNotUseContent; }
            set
            {
                this.doNotUseContent = value;
                OnPropertyChanged();
                this.dialogRelayCommand.NotifyCanExecuteChanged();
            }
        }

        public Guid CorrelationId { get; set; }

        public ICommand DialogCommand { get; private init; }

        public ShellDialogType DialogType
        {
            get { return this.doNotUseDialogType; }
            set
            {
                this.doNotUseDialogType = value;
                OkButtonVisible = DialogType == ShellDialogType.Ok || DialogType == ShellDialogType.OkCancel;
                SaveButtonVisible = DialogType == ShellDialogType.SaveCancel;
                CancelButtonVisible = DialogType != ShellDialogType.Ok;
                OnPropertyChanged(nameof(ActionToolTip));
                OnPropertyChanged(nameof(CloseToolTip));
                OnPropertyChanged();
            }
        }

        public bool HelpButtonVisible
        {
            [UsedImplicitly] get { return this.doNotUseHelpButtonVisible; }
            set
            {
                this.doNotUseHelpButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public bool OkButtonVisible
        {
            get { return this.doNotUseOkButtonVisible; }
            set
            {
                this.doNotUseOkButtonVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OkIsCancel));
            }
        }

        public bool OkIsCancel => OkButtonVisible && !CancelButtonVisible && !SaveButtonVisible;

        public bool SaveButtonVisible
        {
            get { return this.doNotUseSaveButtonVisible; }
            set
            {
                this.doNotUseSaveButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            [UsedImplicitly] get { return this.doNotUseTitle; }
            set
            {
                this.doNotUseTitle = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///    Intended to be called from the <see cref="ShellController"/> so it can orchestrate showing the dialog.
        ///    Each mega-tab of the application has its own instance of this <see cref="ShellDialogController"/>.
        /// </summary>
        /// <param name="message">The message containing all the parameters to configure and show the dialog.</param>
        public void ShowFromShell(ShellDialogRequestMessage message)
        {
            Title = message.Title;
            Content = message.Content;
            DialogType = message.DialogType;
            CorrelationId = message.CorrelationId;
            HelpButtonVisible = message.HelpAvailable;
        }
        
        private bool CanExecuteDialogCommand(ShellDialogButton arg)
        {
            if (Content == null)
            {
                return false;
            }

            var dialogInteractivity = Content as IShellDialogInteractivity;
            if (dialogInteractivity == null)
            {
                return true;
            }

            switch (arg)
            {
                case ShellDialogButton.Cancel:
                    return dialogInteractivity.CanExecuteCancelButton;
                case ShellDialogButton.Ok:
                    return dialogInteractivity.CanExecuteOkButton;
                case ShellDialogButton.Save:
                    return dialogInteractivity.CanExecuteSaveButton;
            }

            return true;
        }

        private void OnDialogCommandExecute(ShellDialogButton commandType)
        {
            // Delay execution so that keyed events happen
            Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    // No correlation id given so no response is expected.
                    if (CorrelationId != Guid.Empty)
                    {
                        switch (commandType)
                        {
                            case ShellDialogButton.Ok:
                            case ShellDialogButton.Save:
                                if (!OkButtonVisible && commandType == ShellDialogButton.Ok) commandType = ShellDialogButton.Save;
                                // Ok will be the default response if Enter is pressed, even when the Ok button is invisble.
                                Messenger.Send(new ShellDialogResponseMessage(Content, commandType) { CorrelationId = CorrelationId });
                                break;

                            case ShellDialogButton.Cancel:
                                Messenger.Send(new ShellDialogResponseMessage(Content, ShellDialogButton.Cancel) { CorrelationId = CorrelationId });
                                break;

                            case ShellDialogButton.Help:
                                Messenger.Send(new ShellDialogResponseMessage(Content, ShellDialogButton.Help) { CorrelationId = CorrelationId });
                                // Don't close the dialog after this button click is processed.
                                return;

                            default:
                                throw new NotSupportedException("Unsupported command type received from Shell Dialog on Shell view. " + commandType);
                        }
                    }

                    // Setting the content to null will hide the dialog, its visibility is bound to the Content != null
                    Content = null;
                });
        }
    }
}