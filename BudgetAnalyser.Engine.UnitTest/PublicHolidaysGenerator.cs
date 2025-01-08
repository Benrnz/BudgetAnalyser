namespace BudgetAnalyser.Engine.UnitTest;

[TestClass]
public class PublicHolidaysGenerator
{
    [TestMethod]
    public void GeneratePublicHolidaysTests()
    {
        Console.WriteLine(PublicHolidaysGeneratorEngine.Generate());
    }
}
