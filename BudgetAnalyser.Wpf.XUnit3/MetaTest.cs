using System.Linq;
using System.Reflection;
using Shouldly;

namespace BudgetAnalyser.Wpf.XUnit3;

public class MetaTest(ITestOutputHelper testOutputHelper)
{
    private const int MinimumTestCount = 126;

    [Fact]
    public void ListAllTests()
    {
        var count = DiscoverTestCount();
        testOutputHelper.WriteLine($"{count} tests discovered.");
    }

    [Fact]
    public void NoDecreaseInTests()
    {
        testOutputHelper.WriteLine($"Minimum test count: {MinimumTestCount}");
        var testCount = DiscoverTestCount();
        testOutputHelper.WriteLine($"There are {testCount} tests.");
        testCount.ShouldBeGreaterThanOrEqualTo(MinimumTestCount);
    }

    private int DiscoverTestCount()
    {
        var assembly = GetType().Assembly;
        return assembly.ExportedTypes
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Count(m => m.GetCustomAttribute<FactAttribute>() is not null);
    }
}
