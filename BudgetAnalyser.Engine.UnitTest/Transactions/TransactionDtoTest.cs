using BudgetAnalyser.Engine.Transactions.Data;

namespace BudgetAnalyser.Engine.UnitTest.Transactions;

[TestClass]
public class TransactionDtoTest
{
    [TestMethod]
    [Description("A test designed to break when new propperties are added to the TransactionDto. This is a trigger to update the mappers.")]
    public void NumberOfPropertiesShouldBe10()
    {
        var dataProperties = typeof(TransactionDto).CountProperties();
        Assert.AreEqual(10, dataProperties);
    }
}
