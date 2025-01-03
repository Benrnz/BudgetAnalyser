﻿using System.Collections.Generic;
using System.Linq;

namespace BudgetAnalyser.Engine.Reports
{
    /// <summary>
    ///     A data storage class to store graph data.  One instance of this class can store multiple lines/series for a graph.
    /// </summary>
    public class GraphData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GraphData" /> class.
        /// </summary>
        public GraphData()
        {
            SeriesList = new List<SeriesData>();
        }

        /// <summary>
        ///     Calculates the smallest value from all the series stored in <see cref="Series" />.
        /// </summary>
        public decimal GraphMinimumValue => SeriesList.Min(s => s.MinimumValue);

        /// <summary>
        ///     The Graph Title
        /// </summary>
        public string GraphName { get; set; }

        /// <summary>
        ///     A list of data series, one for each line/series on the graph.
        /// </summary>
        public IEnumerable<SeriesData> Series => SeriesList;

        internal IList<SeriesData> SeriesList { get; set; }
    }
}
