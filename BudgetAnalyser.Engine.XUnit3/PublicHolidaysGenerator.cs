namespace BudgetAnalyser.Engine.XUnit;

public class PublicHolidaysGenerator
{
    [Fact]
    public void GeneratePublicHolidaysTests()
    {
        Console.WriteLine(PublicHolidaysGeneratorEngine.Generate());
    }
}
