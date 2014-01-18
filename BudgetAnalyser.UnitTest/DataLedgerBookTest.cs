using System;
using System.Xaml;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class DataLedgerBookTest
    {
        [TestMethod]
        public void OutputLedgerBookTestData1()
        {
            var testData = DataLedgerBookTestData.TestData1();
            string serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }

        [TestMethod]
        public void OutputLedgerBookTestData2()
        {
            var testData = DataLedgerBookTestData.TestData2();
            string serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }
    }
}
