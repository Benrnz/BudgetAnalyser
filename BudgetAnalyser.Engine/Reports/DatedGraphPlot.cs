using System;
using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A data storage class to store one single plot on a two dimensional graph.
    ///     Where the x axis is a Date line and the y axis is a monetary amount.
    /// </summary>
    public class DatedGraphPlot
    {
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }

        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)", Justification = "Ok for now, only english is supported")]
        public  string Month => Date.ToString("MMM yy");
    }
}