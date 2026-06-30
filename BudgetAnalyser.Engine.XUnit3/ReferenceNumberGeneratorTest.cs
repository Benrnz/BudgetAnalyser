using System.Diagnostics;
using Shouldly;

namespace BudgetAnalyser.Engine.XUnit;

public class ReferenceNumberGeneratorTest
{
    [Fact]
    public void DuplicateReferenceNumberTest()
    {
        var timer = Stopwatch.StartNew();
        // ReSharper disable once CollectionNeverQueried.Local
        var duplicateCheck = new Dictionary<string, string>();
        for (var i = 0; i < 1000; i++)
        {
            var result = ReferenceNumberGenerator.IssueTransactionReferenceNumber();
            result.ShouldNotBeNull();
            duplicateCheck.Add(result, result);
        }

        timer.Stop();
        Debug.WriteLine($"Completed in {timer.ElapsedTicks}t");
    }
}
