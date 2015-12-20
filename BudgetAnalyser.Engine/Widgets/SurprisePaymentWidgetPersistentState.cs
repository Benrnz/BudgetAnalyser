using System;

namespace BudgetAnalyser.Engine.Widgets
{
    /// <summary>
    /// A Dto to persist a <see cref="SurprisePaymentWidget"/>
    /// </summary>
    /// <seealso cref="BudgetAnalyser.Engine.Widgets.MultiInstanceWidgetState" />
    public class SurprisePaymentWidgetPersistentState : MultiInstanceWidgetState
    {
        /// <summary>
        /// Gets or sets the frequency of the payment.
        /// </summary>
        public WeeklyOrFortnightly Frequency { get; set; }
        /// <summary>
        /// Gets or sets the payment start date.
        /// </summary>
        public DateTime PaymentStartDate { get; set; }
    }
}