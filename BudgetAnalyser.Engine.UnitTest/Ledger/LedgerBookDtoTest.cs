using BudgetAnalyser.Engine.Ledger.Data;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Portable.Xaml;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class LedgerBookDtoTest
    {
        [TestMethod]
        public void OutputLedgerBookTestData1()
        {
            var testData = LedgerBookDtoTestData.TestData1();
            var serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }

        [TestMethod]
        public void OutputLedgerBookTestData2()
        {
            var testData = LedgerBookDtoTestData.TestData2();
            var serialised = XamlServices.Save(testData);
            Console.WriteLine(serialised);
        }
    }
}
