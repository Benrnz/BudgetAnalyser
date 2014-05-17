using System;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using Rees.Wpf;

namespace BudgetAnalyser.ShellDialog
{
    public class ShellDialogController : ControllerBase
    {
        private bool doNotUseCancelButtonVisible;

        private object doNotUseContent;
        private ShellDialogType doNotUseDialogType;
        private bool doNotUseOkButtonVisible;
        private bool doNotUseSaveButtonVisible;
        private string doNotUseTitle;

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
                RaisePropertyChanged(() => CancelButtonVisible);
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
                RaisePropertyChanged(() => Content);
            }
        }

        public Guid CorrelationId { get; set; }

        public ICommand DialogCommand
        {
            get { return new RelayCommand<ShellDialogButton>(OnDialogCommandExecute, CanExecuteDialogCommand); }
        }

        public ShellDialogType DialogType
        {
            get { return this.doNotUseDialogType; }
            set
            {
                this.doNotUseDialogType = value;
                OkButtonVisible = DialogType == ShellDialogType.Ok || DialogType == ShellDialogType.OkCancel;
                SaveButtonVisible = DialogType == ShellDialogType.SaveCancel;
                CancelButtonVisible = DialogType != ShellDialogType.Ok;
                RaisePropertyChanged(() => ActionToolTip);
                RaisePropertyChanged(() => CloseToolTip);
            }
        }

        public bool OkButtonVisible
        {
            get { return this.doNotUseOkButtonVisible; }
            set
            {
                this.doNotUseOkButtonVisible = value;
                RaisePropertyChanged(() => OkButtonVisible);
                RaisePropertyChanged(() => OkIsCancel);
            }
        }

        public bool OkIsCancel
        {
            get { return OkButtonVisible && !CancelButtonVisible && !SaveButtonVisible; }
        }

        public bool SaveButtonVisible
        {
            get { return this.doNotUseSaveButtonVisible; }
            set
            {
                this.doNotUseSaveButtonVisible = value;
                RaisePropertyChanged(() => SaveButtonVisible);
            }
        }

        public string Title
        {
            get { return this.doNotUseTitle; }
            set
            {
                this.doNotUseTitle = value;
                RaisePropertyChanged(() => Title);
            }
        }

        private bool CanExecuteDialogCommand(ShellDialogButton arg)
        {
            bool baseResult = Content != null;
            if (!baseResult)
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
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () =>
            {
                switch (commandType)
                {
                    case ShellDialogButton.Ok:
                    case ShellDialogButton.Save:
                        MessengerInstance.Send(new ShellDialogResponseMessage(Content, ShellDialogButton.Ok) { CorrelationId = CorrelationId });
                        break;

                    case ShellDialogButton.Cancel:
                        MessengerInstance.Send(new ShellDialogResponseMessage(Content, ShellDialogButton.Cancel) { CorrelationId = CorrelationId });
                        break;

                    default:
                        throw new NotSupportedException("Unsupported command type received from Shell Dialog on Shell view. " + commandType);
                }

                Content = null;
            });
        }
    }
}