using System;

namespace BudgetAnalyser.Engine.Reports
{
    public class ReportTransaction
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Narrative { get; set; }
    }
}