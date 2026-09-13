using BudgetAnalyser.Engine.Transactions;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class TransactionTest
{
    [Fact]
    public void NumberOfPropertiesShouldBe11()
    {
        var dataProperties = typeof(Transaction).CountProperties();
        dataProperties.ShouldBe(11);
    }
}
