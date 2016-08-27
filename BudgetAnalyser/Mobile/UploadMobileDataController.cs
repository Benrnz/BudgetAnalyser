using System;
using System.Threading.Tasks;
using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Widgets;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Mobile
{
    public class UploadMobileDataController : ControllerBase
    {
        private readonly IMobileDataExporter dataExporter;
        private readonly IMobileDataUploader uploader;

        public UploadMobileDataController(IMessenger messenger, IMobileDataExporter dataExporter, IMobileDataUploader uploader)
        {
            if (messenger == null) throw new ArgumentNullException(nameof(messenger));
            if (dataExporter == null) throw new ArgumentNullException(nameof(dataExporter));
            if (uploader == null) throw new ArgumentNullException(nameof(uploader));
            this.dataExporter = dataExporter;
            this.uploader = uploader;
            MessengerInstance = messenger;

            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
        }

        private async void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            var widget = message.Widget as UpdateMobileDataWidget;
            if (widget != null && widget.Enabled)
            {
                var budget = widget.BudgetCollection.CurrentActiveBudget;
                var export = this.dataExporter.CreateExportObject(widget.StatementModel, budget, widget.LedgerBook, widget.Filter);

                await Task.Run(() => this.dataExporter.SaveCopyAsync(export));

                // TODO Do I care about exception conditions here?
                await Task.Run(() => this.uploader.UploadDataFileAsync(this.dataExporter.Serialise(export)));
            }
        }
    }
}
