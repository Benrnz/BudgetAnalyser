using BudgetAnalyser.Engine.Transactions.Data;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit.Transactions;

public class TransactionDtoTest
{
    [Fact]
    public void NumberOfPropertiesShouldBe10()
    {
        var dataProperties = typeof(TransactionDto).CountProperties();
        dataProperties.ShouldBe(10);
    }
}
