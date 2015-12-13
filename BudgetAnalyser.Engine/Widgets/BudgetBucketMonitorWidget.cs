using System;
using BudgetAnalyser.Engine.Annotations;

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
            DetailedText = BucketCode;
            if (!Enabled)
            {
                ToolTip = this.disabledToolTip;
            }
        }
    }
}