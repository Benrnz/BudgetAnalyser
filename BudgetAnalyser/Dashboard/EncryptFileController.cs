using System;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.Annotations;

namespace BudgetAnalyser.Dashboard
{
    public class EncryptFileController : ControllerBase, IShellDialogInteractivity
    {
        private readonly IApplicationDatabaseService appDbService;
        private readonly Guid dialogCorrelationId = Guid.NewGuid();
        private readonly IUserMessageBox messageService;
        private readonly IUserQuestionBoxYesNo questionService;
        private bool doNotUseEncryptFileMode;
        private bool doNotUseEnterPasswordMode;
        private bool doNotUseHasUnsavedChanges;
        private bool doNotUseIsEncrypted;
        private SecureString password;
        private bool passwordConfirmed;

        public EncryptFileController([NotNull] IUiContext uiContext, [NotNull] IApplicationDatabaseService appDbService)
        {
            this.appDbService = appDbService;
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (appDbService == null) throw new ArgumentNullException(nameof(appDbService));
            this.questionService = uiContext.UserPrompts.YesNoBox;
            this.messageService = uiContext.UserPrompts.MessageBox;

            MessengerInstance = uiContext.Messenger;
            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDiaglogResponseMessageReceived);
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton => true;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton
        {
            get
            {
                if (EncryptFileMode)
                {
                    return this.password != null && this.password.Length > 4 && this.passwordConfirmed;
                }
                else
                {
                    return this.password != null && this.password.Length > 4;
                }
            }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton => false;

        public bool EncryptFileMode
        {
            get { return this.doNotUseEncryptFileMode; }
            private set
            {
                this.doNotUseEncryptFileMode = value;
                this.doNotUseEnterPasswordMode = !value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => EnterPasswordMode);
            }
        }

        public bool EnterPasswordMode
        {
            get { return this.doNotUseEnterPasswordMode; }
            private set
            {
                this.doNotUseEnterPasswordMode = value;
                this.doNotUseEncryptFileMode = !value;
                RaisePropertyChanged();
                RaisePropertyChanged(() => EncryptFileMode);
            }
        }

        public string FileName { get; private set; }

        public bool HasUnsavedChanges
        {
            get { return this.doNotUseHasUnsavedChanges; }
            private set
            {
                this.doNotUseHasUnsavedChanges = value;
                RaisePropertyChanged();
            }
        }

        public bool IsEncrypted
        {
            get { return this.doNotUseIsEncrypted; }
            private set
            {
                this.doNotUseIsEncrypted = value;
                RaisePropertyChanged();
            }
        }

        public void SetConfirmedPassword(bool confirmed)
        {
            this.passwordConfirmed = confirmed;
            CommandManager.InvalidateRequerySuggested();
        }

        public void SetPassword(SecureString newPassword)
        {
            this.password?.Dispose();
            this.password = newPassword;
            CommandManager.InvalidateRequerySuggested();
        }

        public void ShowEnterPasswordDialog(string appDbFileName)
        {
            EnterPasswordMode = true;
            FileName = appDbFileName;
            MessengerInstance.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Enter Password"
            });
        }

        private async Task EncryptFile()
        {
            bool? confirmation;
            if (IsEncrypted)
            {
                confirmation = this.questionService.Show(
                    "Are you sure you want to decrypt and store the data files in plain text?  After this is complete the files can be read by any other program without a password.",
                    "Decrypt Data Files?");
            }
            else
            {
                confirmation = this.questionService.Show(
                    "Are you sure you want to encrypt and protect the data files?  After this is complete the files cannot be read by any other program aside from Budget Analyser. You will be required to enter the password each time you load your Budget Analyser file.",
                    "Encrypt Data Files?");
            }

            if (confirmation == null || !confirmation.Value) return;

            this.appDbService.SetPassword(this.password);

            // TODO Background task?
            await this.appDbService.EncryptFilesAsync();
        }

        private async void OnShellDiaglogResponseMessageReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId)) return;

            if (message.Response == ShellDialogButton.Cancel)
            {
                this.password?.Dispose();
                return;
            }

            if (EncryptFileMode)
            {
                await EncryptFile();
                return;
            }

            if (EnterPasswordMode)
            {
                this.appDbService.SetPassword(this.password);
                MessengerInstance.Send(new PasswordSetMessage());
            }

            this.password = null;
        }

        private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            if (message.Handled || !(message.Widget is EncryptWidget)) return;

            this.passwordConfirmed = false;
            EncryptFileMode = true;
            IsEncrypted = this.appDbService.IsEncrypted;
            HasUnsavedChanges = this.appDbService.HasUnsavedChanges;

            if (HasUnsavedChanges)
            {
                this.messageService.Show("There are unsaved changes. You will need to save these changes before password protecting the data files. Unable to proceed.", "Protect Data Files");
                return;
            }

            MessengerInstance.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Protect Data Files"
            });
        }
    }
}