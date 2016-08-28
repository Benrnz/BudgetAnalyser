using System;
using System.Diagnostics;
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
        private readonly IMobileDataUploader uploader;
        private readonly IUserMessageBox messageBoxService;

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

        private async void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            var widget = message.Widget as UpdateMobileDataWidget;
            if (widget != null && widget.Enabled)
            {
                try
                {
                    widget.LockWhileUploading(true);
                    var budget = widget.BudgetCollection.CurrentActiveBudget;
                    var export = this.dataExporter.CreateExportObject(widget.StatementModel, budget, widget.LedgerBook, widget.Filter);

                    await Task.Run(() => this.dataExporter.SaveCopyAsync(export));

                    // TODO Do I care about exception conditions here?
                    await Task.Run(() => this.uploader.UploadDataFileAsync(this.dataExporter.Serialise(export)));

                    this.messageBoxService.Show("Mobile summary data exported successfully.");
                }
                finally
                {
                    widget.LockWhileUploading(false);
                }
            }
        }
    }
}
