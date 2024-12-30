using BudgetAnalyser.Engine.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class TransactionTest
    {
        [TestMethod]
        [Description("A test designed to break when new propperties are added to the Transaction. This is a trigger to update the mappers.")]
        public void NumberOfPropertiesShouldBe11()
        {
            var dataProperties = typeof(Transaction).CountProperties();
            Assert.AreEqual(11, dataProperties);
        }
    }
}
