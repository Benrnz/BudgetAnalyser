﻿using System.Reflection;
using Shouldly;
using Xunit.Abstractions;

namespace BudgetAnalyser.Engine.XUnit;

public class MetaTest(ITestOutputHelper testOutputHelper)
{
    private const int MinimumTestCount = 185;

    [Fact]
    public void ListAllTests()
    {
        var count = DiscoverTestCount();
        testOutputHelper.WriteLine($"{count} tests discovered.");
    }

    [Fact]
    public void NoDecreaseInTests()
    {
        DiscoverTestCount().ShouldBeGreaterThanOrEqualTo(MinimumTestCount);
    }

    private int DiscoverTestCount()
    {
        var assembly = GetType().Assembly;
        var count = assembly.ExportedTypes
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Count(m => m.GetCustomAttribute<FactAttribute>() is not null);
        return count;
    }
}
