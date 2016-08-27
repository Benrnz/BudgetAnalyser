using BudgetAnalyser.Dashboard;
using BudgetAnalyser.Engine.Mobile;
using BudgetAnalyser.Engine.Widgets;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser.Mobile
{
    public class UploadMobileDataController : ControllerBase
    {
        private readonly MobileDataExporter dataExporter;

        public UploadMobileDataController(IMessenger messenger, MobileDataExporter dataExporter)
        {
            this.dataExporter = dataExporter;
            MessengerInstance = messenger;

            MessengerInstance.Register<WidgetActivatedMessage>(this, OnWidgetActivatedMessageReceived);
        }

        private void OnWidgetActivatedMessageReceived(WidgetActivatedMessage message)
        {
            var widget = message.Widget as UpdateMobileDataWidget;
            if (widget != null && widget.Enabled)
            {
                var budget = widget.BudgetCollection.CurrentActiveBudget;
                var export = this.dataExporter.CreateExportObject(widget.StatementModel, budget, widget.LedgerBook);
            }
        }
    }
}
