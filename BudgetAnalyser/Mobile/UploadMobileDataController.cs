using System;
using System.Collections.Generic;
using System.Security;
using System.Threading.Tasks;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.ShellDialog;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;
using Rees.Wpf.Annotations;

namespace BudgetAnalyser.Mobile
{
    public class UploadMobileDataController : ControllerBase, IShellDialogInteractivity
    {
        private readonly string[] amazonRegions =
        {
            "ap-southeast-2",
            "ap-southeast-1",
            "cn-north-1",
            "eu-central-1",
            "eu-central-1",
            "eu-west-1",
            "sa-east-1",
            "us-east",
            "us-west-1",
            "us-west-2",
            "ap-northeast-1",
            "ap-northeast-2"
        };

        private readonly IApplicationDatabaseService appDbService;
        private readonly Guid correlationId = Guid.NewGuid();
        private readonly IMobileDataExporter dataExporter;
        private readonly ILogger logger;
        private readonly IUserMessageBox messageBoxService;
        private readonly IMobileDataUploader uploader;
        private string doNotUseAccessKeyId;
        private string doNotUseAccessKeySecret;
        private string doNotUseAmazonRegion;
        private UpdateMobileDataWidget widget;

        public UploadMobileDataController(
            [NotNull] IUiContext uiContext,
            [NotNull] IMobileDataExporter dataExporter,
            [NotNull] IMobileDataUploader uploader,
            [NotNull] IApplicationDatabaseService appDbService)
        {
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (dataExporter == null) throw new ArgumentNullException(nameof(dataExporter));
            if (uploader == null) throw new ArgumentNullException(nameof(uploader));
            if (appDbService == null) throw new ArgumentNullException(nameof(appDbService));
            this.dataExporter = dataExporter;
            this.uploader = uploader;
            this.appDbService = appDbService;
            MessengerInstance = uiContext.Messenger;
            this.messageBoxService = uiContext.UserPrompts.MessageBox;
            this.logger = uiContext.Logger;

            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
            MessengerInstance.Register<ShellDialogResponseMessage>(this, OnShellDialogMessageReceived);
        }

        public string AccessKeyId
        {
            get { return this.doNotUseAccessKeyId; }
            set
            {
                this.doNotUseAccessKeyId = value;
                RaisePropertyChanged();
            }
        }

        public string AccessKeySecret
        {
            get { return this.doNotUseAccessKeySecret; }
            set
            {
                this.doNotUseAccessKeySecret = value;
                RaisePropertyChanged();
            }
        }

        public string AmazonRegion
        {
            get { return this.doNotUseAmazonRegion; }
            set
            {
                this.doNotUseAmazonRegion = value;
                RaisePropertyChanged();
            }
        }

        public IEnumerable<string> AmazonRegions => this.amazonRegions;

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

                await Task.Run(() => this.uploader.UploadDataFileAsync(this.dataExporter.Serialise(export), AccessKeyId, AccessKeySecret, AmazonRegion));

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
                var changed = AccessKeyId != this.widget.LedgerBook.MobileSettings.AccessKeyId;
                changed |= AccessKeySecret != this.widget.LedgerBook.MobileSettings.AccessKeySecret;
                changed |= AmazonRegion != this.widget.LedgerBook.MobileSettings.AmazonS3Region;
                if (changed)
                {
                    this.widget.LedgerBook.MobileSettings.AccessKeyId = AccessKeyId;
                    this.widget.LedgerBook.MobileSettings.AccessKeySecret = AccessKeySecret;
                    this.widget.LedgerBook.MobileSettings.AmazonS3Region = AmazonRegion;
                    this.appDbService.NotifyOfChange(ApplicationDataType.Ledger);
                }
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
                AccessKeyId = this.widget.LedgerBook.MobileSettings.AccessKeyId;
                AccessKeySecret = this.widget.LedgerBook.MobileSettings.AccessKeySecret;
                AmazonRegion = this.widget.LedgerBook.MobileSettings.AmazonS3Region;
                MessengerInstance.Send(new ShellDialogRequestMessage(BudgetAnalyserFeature.Dashboard, this, ShellDialogType.OkCancel)
                {
                    CorrelationId = this.correlationId,
                    Title = "Upload Mobile Summary Data"
                });
            }
        }
    }
}