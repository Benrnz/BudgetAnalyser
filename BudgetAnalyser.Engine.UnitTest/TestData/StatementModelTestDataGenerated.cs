using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestHarness;

namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    public static class StatementModelTestDataGenerated
    {
        public static IAccountTypeRepository AccountTypeRepo { get; set; }
        public static IBudgetBucketRepository BudgetBucketRepo { get; set; }

        /// <summary>THIS IS GENERATED CODE </summary>
        [GeneratedCode("StatementModelTestDataGenerator.GenerateCSharp", "11/23/2015 13:04:40")]
        public static StatementModel TestDataGenerated()
        {
            return null;
        }
    }
}