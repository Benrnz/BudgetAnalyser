using System.CodeDom.Compiler;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class StatementModelTestDataGenerated
    {
        public static IAccountTypeRepository AccountTypeRepo { get; set; }
        public static IBudgetBucketRepository BudgetBucketRepo { get; set; }

        /// <summary>THIS IS GENERATED CODE </summary>
        [GeneratedCode("StatementModelTestDataGenerator.GenerateCSharp", "06/06/2014 17:03:50")]
        public static StatementModel TestDataGenerated()
        {
            // Stubbed out - I dont want to check this generated data in.
            return new StatementModel(new FakeLogger());
        }
    }
}