using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public sealed class BudgetBucketMonitorWidget : RemainingBudgetBucketWidget, IUserDefinedWidget
    {
        private readonly string disabledToolTip;
        private string doNotUseId;

        public BudgetBucketMonitorWidget() 
        {
            this.disabledToolTip = "Either a Statement, Budget, or a Filter are not present, or the Bucket Code is not valid, remaining budget cannot be calculated.";
        }

        public string Id
        {
            get { return this.doNotUseId; }
            set
            {
                this.doNotUseId = value;
                OnPropertyChanged();
                BucketCode = Id;
            }
        }

        public Type WidgetType => GetType();

        public void Initialise(MultiInstanceWidgetState state, ILogger logger)
        {
        }

        public override void Update([NotNull] params object[] input)
        {
            base.Update(input);

            if (!Enabled)
            {
                ToolTip = this.disabledToolTip;
                return;
            }
        }
    }
}