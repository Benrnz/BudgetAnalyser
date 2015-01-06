using System;

namespace BudgetAnalyser.Engine.Widgets
{
    public class SurprisePaymentWidgetPersistentState : MultiInstanceWidgetState
    {
        public DateTime PaymentStartDate { get; set; }
        public WeeklyOrFortnightly Frequency { get; set; }
    }
}