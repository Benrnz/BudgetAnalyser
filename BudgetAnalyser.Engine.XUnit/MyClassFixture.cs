namespace BudgetAnalyser.Engine.XUnit;

/// <summary>
///     Sometimes useful to share data, connection, or anything else between a group of tests.
///     See also <see cref="CollectionAttribute"/>
/// </summary>
public class MyClassFixture
{
    public Guid JustAnExmaple { get; } = Guid.NewGuid();
}
