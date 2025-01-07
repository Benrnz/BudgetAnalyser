using FluentAssertions;

namespace BudgetAnalyser.Engine.XUnit;

public class UnitTest1(MyClassFixture sharedFixture) : IClassFixture<MyClassFixture>
{
    private readonly MyClassFixture fixture = sharedFixture;

    [Fact]
    public void Test1()
    {
        var testResult = true;
        testResult.Should().BeTrue();
    }
}
