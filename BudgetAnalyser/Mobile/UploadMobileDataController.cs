using System;
using System.Security;
using System.Threading.Tasks;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Widgets;
using Rees.UserInteraction.Contracts;
using Rees.Wpf;

namespace BudgetAnalyser.Mobile
{
    public class UploadMobileDataController : ControllerBase
    {
        private readonly IMobileDataExporter dataExporter;
        private readonly IUserMessageBox messageBoxService;
        private readonly IMobileDataUploader uploader;

        public UploadMobileDataController(IUiContext uiContext, IMobileDataExporter dataExporter, IMobileDataUploader uploader)
        {
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (dataExporter == null) throw new ArgumentNullException(nameof(dataExporter));
            if (uploader == null) throw new ArgumentNullException(nameof(uploader));
            this.dataExporter = dataExporter;
            this.uploader = uploader;
            MessengerInstance = uiContext.Messenger;
            this.messageBoxService = uiContext.UserPrompts.MessageBox;

            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
        }

        public string AccessKeyId { get; set; }

        public string AccessKeySecret { get; set; }

        private async void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            // TODO Show UI to upload and specify credentials.
            var widget = message.Widget as UpdateMobileDataWidget;
            if (widget != null && widget.Enabled)
            {
                try
                {
                    widget.LockWhileUploading(true);
                    var budget = widget.BudgetCollection.CurrentActiveBudget;
                    var export = this.dataExporter.CreateExportObject(widget.StatementModel, budget, widget.LedgerBook, widget.Filter);

                    await Task.Run(() => this.dataExporter.SaveCopyAsync(export));

                    await Task.Run(() => this.uploader.UploadDataFileAsync(this.dataExporter.Serialise(export), AccessKeyId, AccessKeySecret));

                    this.messageBoxService.Show("Mobile summary data exported successfully.");
                }
                catch (SecurityException)
                {
                    // TODO
                }
                catch (Exception)
                {
                    // TODO
                }
                finally
                {
                    widget.LockWhileUploading(false);
                }
            }
        }
    }
}