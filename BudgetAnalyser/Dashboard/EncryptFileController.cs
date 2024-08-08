using System.Security;
using System.Windows.Input;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using CommunityToolkit.Mvvm.Messaging;
using Rees.Wpf.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
    public class EncryptFileController : ControllerBase, IShellDialogInteractivity
    {
        private readonly IApplicationDatabaseService appDbService;
        private readonly Guid dialogCorrelationId = Guid.NewGuid();
        private readonly IUserMessageBox messageService;
        private readonly IUserQuestionBoxYesNo questionService;
        private bool doNotUseDecryptFileMode;
        private bool doNotUseEncryptFileMode;
        private bool doNotUseEnterPasswordMode;
        private bool doNotUseHasUnsavedChanges;
        private bool doNotUseIsEncrypted;
        private SecureString password;
        private bool passwordConfirmed;
        private string doNotUseValidationMessage;

        public EncryptFileController([NotNull] IUiContext uiContext, [NotNull] IApplicationDatabaseService appDbService) : base(uiContext.Messenger)
        {
            this.appDbService = appDbService;
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (appDbService == null) throw new ArgumentNullException(nameof(appDbService));
            this.questionService = uiContext.UserPrompts.YesNoBox;
            this.messageService = uiContext.UserPrompts.MessageBox;

            Messenger.Register<EncryptFileController, WidgetActivatedMessage>(this, static (r, m) => r.OnWidgetActivatedMessageReceived(m));
            Messenger.Register<EncryptFileController, ShellDialogResponseMessage>(this, static (r, m) => r.OnShellDiaglogResponseMessageReceived(m));
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

                return this.password != null && this.password.Length > 4;
            }
        }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton => false;

        public bool DecryptFileMode
        {
            get { return this.doNotUseDecryptFileMode; }
            private set
            {
                this.doNotUseDecryptFileMode = value;
                this.doNotUseEncryptFileMode = !value;
                this.doNotUseEnterPasswordMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnterPasswordMode));
                OnPropertyChanged(nameof(EncryptFileMode));
            }
        }

        public bool EncryptFileMode
        {
            get { return this.doNotUseEncryptFileMode; }
            private set
            {
                this.doNotUseEncryptFileMode = value;
                this.doNotUseEnterPasswordMode = !value;
                this.doNotUseDecryptFileMode = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnterPasswordMode));
                OnPropertyChanged(nameof(DecryptFileMode));
            }
        }

        public bool EnterPasswordMode
        {
            get { return this.doNotUseEnterPasswordMode; }
            private set
            {
                this.doNotUseEnterPasswordMode = value;
                this.doNotUseEncryptFileMode = !value;
                this.doNotUseDecryptFileMode = !value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EncryptFileMode));
                OnPropertyChanged(nameof(DecryptFileMode));
            }
        }

        public string EnterPasswordText { get; private set; }

        public string FileName { get; private set; }

        public bool HasUnsavedChanges
        {
            get { return this.doNotUseHasUnsavedChanges; }
            private set
            {
                this.doNotUseHasUnsavedChanges = value;
                OnPropertyChanged();
            }
        }

        public bool IsEncrypted
        {
            get { return this.doNotUseIsEncrypted; }
            private set
            {
                this.doNotUseIsEncrypted = value;
                OnPropertyChanged();
            }
        }

        public string ValidationMessage
        {
            get { return this.doNotUseValidationMessage; }
            private set
            {
                this.doNotUseValidationMessage = value;
                OnPropertyChanged();
            }
        }

        public void ShowDecryptFilesDialog(string validationMessage = "")
        {
            ValidationMessage = validationMessage;
            IsEncrypted = true;
            DecryptFileMode = true;
            EnterPasswordText = "Confirm your password to decrypt your files";
            ShowEncryptDecryptDialogCommon();
        }

        public void ShowEncryptFilesDialog()
        {
            ValidationMessage = string.Empty;
            IsEncrypted = false;
            EncryptFileMode = true;
            ShowEncryptDecryptDialogCommon();
        }

        public void ShowEnterPasswordDialog(string appDbFileName, string validationMessage = "")
        {
            EnterPasswordMode = true;
            FileName = appDbFileName;
            EnterPasswordText = "Please enter your password for ";
            ValidationMessage = validationMessage;
            Messenger.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Enter Password"
            });
        }

        internal void SetConfirmedPassword(bool confirmed)
        {
            this.passwordConfirmed = confirmed;
            CommandManager.InvalidateRequerySuggested();
        }

        internal void SetPassword(SecureString newPassword)
        {
            this.password?.Dispose();
            this.password = newPassword;
            CommandManager.InvalidateRequerySuggested();
        }

        private async Task DecryptFiles()
        {
            try
            {
                bool? confirmation = this.questionService.Show(
                    "Are you sure you want to decrypt and store the data files in plain text?  After this is complete the files can be read by any other program without a password.",
                    "Decrypt Data Files?");
                if (confirmation == null || !confirmation.Value) return;
                await Task.Run(async () => await this.appDbService.DecryptFilesAsync(this.password));
                this.messageService.Show("Files are decrypted and are saved as plain CSV and XML files. They are no longer protected.  Encrypted files have been deleted.",
                    "Decrypt Data Files - Completed");
            }
            catch (EncryptionKeyIncorrectException)
            {
                ShowDecryptFilesDialog("Incorrect password.");
            }
        }

        private async Task EncryptFiles()
        {
            bool? confirmation = this.questionService.Show(
                "Are you sure you want to encrypt and protect the data files?  After this is complete the files cannot be read by any other program aside from Budget Analyser. You will be required to enter the password each time you load your Budget Analyser file.",
                "Encrypt Data Files?");
            if (confirmation == null || !confirmation.Value) return;
            this.appDbService.SetCredential(this.password);
            await Task.Run(async () => await this.appDbService.EncryptFilesAsync());
            this.messageService.Show(
                "Files are now protected with strong encryption. DO NOT FORGET YOUR PASSWORD. Files are not recoverable without the correct password. Backup files have been created alongside your existing data files. These can be deleted when your satisfied with the encryption.",
                "Encrypt Data Files - Completed");
        }

        private async void OnShellDiaglogResponseMessageReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.dialogCorrelationId)) return;

            if (message.Response == ShellDialogButton.Cancel)
            {
                this.password?.Dispose();
                return;
            }

            try
            {
                if (EncryptFileMode)
                {
                    await EncryptFiles();
                    return;
                }

                if (DecryptFileMode)
                {
                    await DecryptFiles();
                    return;
                }

                if (EnterPasswordMode)
                {
                    this.appDbService.SetCredential(this.password);
                    Messenger.Send(new PasswordSetMessage { DatabaseStorageKey = FileName });
                    // Incorrect password is handled by calling code.
                }
            }
            finally
            {
                this.password = null;
            }
        }

        private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            if (message.Handled || !(message.Widget is EncryptWidget)) return;

            if (this.appDbService.IsEncrypted)
            {
                ShowDecryptFilesDialog();
            }
            else
            {
                ShowEncryptFilesDialog();
            }
        }

        private void ShowEncryptDecryptDialogCommon()
        {
            this.passwordConfirmed = false;
            HasUnsavedChanges = this.appDbService.HasUnsavedChanges;
            FileName = string.Empty;

            if (HasUnsavedChanges)
            {
                this.messageService.Show("There are unsaved changes. You will need to save these changes before password protecting the data files. Unable to proceed.", "Protect Data Files");
                return;
            }

            Messenger.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
            {
                CorrelationId = this.dialogCorrelationId,
                Title = "Protect Data Files"
            });
        }
    }
}