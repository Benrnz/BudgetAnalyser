using System;

namespace BudgetAnalyser.Engine.Reports
{
    public class DatedGraphPlot
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        public string Month
        {
            get { return Date.ToString("MMM yy"); }
        }
    }
}