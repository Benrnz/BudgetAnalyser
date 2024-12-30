using System;
using System.Collections.Generic;

namespace BudgetAnalyser.Engine.UnitTest
{
    public static class PublicHolidaysTestData
    {
        public static IDictionary<int, IEnumerable<DateTime>> ExpectedHolidays { get; } = new Dictionary<int, IEnumerable<DateTime>>
        {
            [2014] =
                new List<DateTime>
                {
                    new DateTime(2014, 1, 1),
                    new DateTime(2014, 1, 2),
                    new DateTime(2014, 1, 27),
                    new DateTime(2014, 2, 6),
                    new DateTime(2014, 4, 18),
                    new DateTime(2014, 4, 21),
                    new DateTime(2014, 4, 25),
                    new DateTime(2014, 6, 2),
                    new DateTime(2014, 10, 27),
                    new DateTime(2014, 12, 25),
                    new DateTime(2014, 12, 26)
                },
            [2015] =
                new List<DateTime>
                {
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 2),
                    new DateTime(2015, 1, 26),
                    new DateTime(2015, 2, 6),
                    new DateTime(2015, 4, 3),
                    new DateTime(2015, 4, 6),
                    new DateTime(2015, 4, 27),
                    new DateTime(2015, 6, 1),
                    new DateTime(2015, 10, 26),
                    new DateTime(2015, 12, 25),
                    new DateTime(2015, 12, 28)
                },
            [2016] =
                new List<DateTime>
                {
                    new DateTime(2016, 1, 1),
                    new DateTime(2016, 1, 4),
                    new DateTime(2016, 2, 1),
                    new DateTime(2016, 2, 8),
                    new DateTime(2016, 3, 25),
                    new DateTime(2016, 3, 28),
                    new DateTime(2016, 4, 25),
                    new DateTime(2016, 6, 6),
                    new DateTime(2016, 10, 24),
                    new DateTime(2016, 12, 26),
                    new DateTime(2016, 12, 27)
                },
            [2017] =
                new List<DateTime>
                {
                    new DateTime(2017, 1, 2),
                    new DateTime(2017, 1, 3),
                    new DateTime(2017, 1, 30),
                    new DateTime(2017, 2, 6),
                    new DateTime(2017, 4, 14),
                    new DateTime(2017, 4, 17),
                    new DateTime(2017, 4, 25),
                    new DateTime(2017, 6, 5),
                    new DateTime(2017, 10, 23),
                    new DateTime(2017, 12, 25),
                    new DateTime(2017, 12, 26)
                },
            [2018] =
                new List<DateTime>
                {
                    new DateTime(2018, 1, 1),
                    new DateTime(2018, 1, 2),
                    new DateTime(2018, 1, 29),
                    new DateTime(2018, 2, 6),
                    new DateTime(2018, 3, 30),
                    new DateTime(2018, 4, 2),
                    new DateTime(2018, 4, 25),
                    new DateTime(2018, 6, 4),
                    new DateTime(2018, 10, 22),
                    new DateTime(2018, 12, 25),
                    new DateTime(2018, 12, 26)
                }
        };
    }
}
