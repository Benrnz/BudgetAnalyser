using System.Diagnostics;

namespace BudgetAnalyser.Engine.UnitTest;

[TestClass]
public class ReferenceNumberGeneratorTest
{
    [TestMethod]
    public void DuplicateReferenceNumberTest()
    {
        var timer = Stopwatch.StartNew();
        // ReSharper disable once CollectionNeverQueried.Local
        var duplicateCheck = new Dictionary<string, string>();
        for (var i = 0; i < 1000; i++)
        {
            var result = ReferenceNumberGenerator.IssueTransactionReferenceNumber();
            //Debug.WriteLine($"{i} {result}");
            Assert.IsNotNull(result);
            duplicateCheck.Add(result, result);
        }

        timer.Stop();
        Debug.WriteLine($"Completed in {timer.ElapsedTicks}t");
    }
}
