using System;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    /// A data storage class to store one single plot on a two dimensional graph.
    /// Where the x axis is a Date line and the y axis is a monetary amount.
    /// </summary>
    public class DatedGraphPlot
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)", Justification = "Ok for now, only english is supported")]
        public string Month
        {
            get { return Date.ToString("MMM yy"); }
        }
    }
}