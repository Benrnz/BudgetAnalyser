using System;
using System.Xaml;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookDtoTest
    {
        [TestMethod]
        public void OutputLedgerBookTestData1()
        {
            var testData = LedgerBookDtoTestData.TestData1();
            string serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }

        [TestMethod]
        public void OutputLedgerBookTestData2()
        {
            var testData = LedgerBookDtoTestData.TestData2();
            string serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }
    }
}
