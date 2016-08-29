using System;
using System.Security;
using System.Threading.Tasks;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Mobile
{
    public class UploadMobileDataController : ControllerBase, IShellDialogInteractivity
    {
        private readonly Guid correlationId = Guid.NewGuid();
        private readonly IMobileDataExporter dataExporter;
        private readonly IUserMessageBox messageBoxService;
        private readonly IMobileDataUploader uploader;
        private UpdateMobileDataWidget widget;
        private readonly ILogger logger;

        public UploadMobileDataController(IUiContext uiContext, IMobileDataExporter dataExporter, IMobileDataUploader uploader)
        {
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (dataExporter == null) throw new ArgumentNullException(nameof(dataExporter));
            if (uploader == null) throw new ArgumentNullException(nameof(uploader));
            this.dataExporter = dataExporter;
            this.uploader = uploader;
            MessengerInstance = uiContext.Messenger;
            this.messageBoxService = uiContext.UserPrompts.MessageBox;
            this.logger = uiContext.Logger;

            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogMessageReceived);
        }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteCancelButton => true;

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteOkButton => AccessKeyId.IsSomething() && AccessKeySecret.IsSomething();

        /// <summary>
        ///     Will be called to ascertain the availability of the button.
        /// </summary>
        public bool CanExecuteSaveButton => false;

        private async Task AttemptUploadAsync()
        {
            try
            {
                this.widget.LockWhileUploading(true);
                var budget = this.widget.BudgetCollection.CurrentActiveBudget;
                var export = this.dataExporter.CreateExportObject(this.widget.StatementModel, budget, this.widget.LedgerBook, this.widget.Filter);

                await Task.Run(() => this.dataExporter.SaveCopyAsync(export));

                await Task.Run(() => this.uploader.UploadDataFileAsync(this.dataExporter.Serialise(export), AccessKeyId, AccessKeySecret));

                this.messageBoxService.Show("Mobile summary data exported successfully.");
            }
            catch (SecurityException ex)
            {
                this.logger.LogError(ex, l => "A Security Exception occured attempting to upload Mobile Summary Data.");
                this.messageBoxService.Show(ex.Message, "Invalid Credentials");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, l => "An unexpected Exception occured attempting to upload Mobile Summary Data.");
                this.messageBoxService.Show(ex.Message, "Upload Failed");
            }
            finally
            {
                this.widget.LockWhileUploading(false);
            }
        }

        private async void OnShellDialogMessageReceived(ShellDialogResponseMessage message)
        {
            if (!message.IsItForMe(this.correlationId)) return;

            try
            {
                if (message.Response == ShellDialogButton.Cancel) return;
                await AttemptUploadAsync();
            }
            finally
            {
                this.widget = null;
            }
        }

        private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            this.widget = message.Widget as UpdateMobileDataWidget;
            if (this.widget != null && this.widget.Enabled)
            {
                MessengerInstance.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel));
            }
        }
    }
}