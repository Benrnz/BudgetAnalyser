using System;

namespace BudgetAnalyser.Engine.Widgets
{
    public class SurprisePaymentWidgetPersistentState : MultiInstanceWidgetState
    {
        public WeeklyOrFortnightly Frequency { get; set; }
        public DateTime PaymentStartDate { get; set; }
    }
}