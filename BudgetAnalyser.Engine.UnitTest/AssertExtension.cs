namespace BudgetAnalyser.Engine.UnitTest;

public static class AssertExtensions
{
    public static void AreEqualWithTolerance(decimal expected, decimal actual, decimal tolerance = 0.01M, string message = "")
    {
        if (Math.Abs(expected - actual) >= tolerance)
        {
            throw new AssertFailedException($"Expected: {expected}, Actual: {actual}. {message}");
        }
    }

    public static void AreEqualWithTolerance(double expected, double actual, double tolerance = 0.01, string message = "")
    {
        if (Math.Abs(expected - actual) >= tolerance)
        {
            throw new AssertFailedException($"Expected: {expected}, Actual: {actual}. {message}");
        }
    }
}
