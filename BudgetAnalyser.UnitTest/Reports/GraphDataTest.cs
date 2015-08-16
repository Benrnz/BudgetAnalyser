using System;
using BudgetAnalyser.Engine.Reports;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Reports
{
    [TestClass]
    public class GraphDataTest
    {
        private SeriesData Series1 { get; set; }
        private SeriesData Series2 { get; set; }
        private SeriesData Series3 { get; set; }
        private DateTime StartDate { get; set; }
        private GraphData Subject { get; set; }

        [TestMethod]
        public void GraphMinimumValueShouldReturn0GivenPostiveDataBeginningWithZero()
        {
            Assert.AreEqual(0, Subject.GraphMinimumValue);
        }

        [TestMethod]
        public void GraphMinimumValueShouldReturnMinus20GivenDataWithSmallestValueOfMinus20()
        {
            Series3.PlotsList[2].Amount = -20;
            Assert.AreEqual(-20, Subject.GraphMinimumValue);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            StartDate = new DateTime(2014, 08, 20);
            Subject = new GraphData
            {
                GraphName = "Main Graph Title"
            };

            Series1 = new SeriesData
            {
                SeriesName = "Series 1",
                Description = "Series 1 Description"
            };

            Series2 = new SeriesData
            {
                SeriesName = "Series 2",
                Description = "Series 2 Description"
            };

            Series3 = new SeriesData
            {
                SeriesName = "Series 3",
                Description = "Series 3 Description"
            };

            Subject.SeriesList.Add(Series1);
            Subject.SeriesList.Add(Series2);
            Subject.SeriesList.Add(Series3);

            var seriesNumber = 0;
            foreach (SeriesData series in Subject.SeriesList)
            {
                for (var index = 0; index < 31; index++)
                {
                    series.PlotsList.Add(new DatedGraphPlot { Amount = seriesNumber * 100 * (index + 1), Date = StartDate.AddDays(index) });
                }

                seriesNumber++;
            }
        }

        [TestMethod]
        public void VisibleShouldDefaultToTrue()
        {
            Assert.IsTrue(Series1.Visible);
        }

        [TestMethod]
        public void VisibleShouldRaiseChangeEventWhenVisibleChanges()
        {
            var eventRaised = false;
            Series1.PropertyChanged += (s, e) => eventRaised = true;
            Series1.Visible = false;
            Assert.IsTrue(eventRaised);
        }
    }
}