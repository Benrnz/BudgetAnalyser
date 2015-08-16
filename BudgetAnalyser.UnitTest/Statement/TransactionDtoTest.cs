using BudgetAnalyser.Engine.Statement.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Statement
{
    [TestClass]
    public class TransactionDtoTest
    {
        [TestMethod]
        [Description("A test designed to break when new propperties are added to the TransactionDto. This is a trigger to update the mappers.")]
        public void NumberOfPropertiesShouldBe10()
        {
            int dataProperties = typeof(TransactionDto).CountProperties();
            Assert.AreEqual(10, dataProperties);
        }
    }
}