using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest
{
    [TestClass]
    public class ReferenceNumberGeneratorTest
    {
        [TestMethod]
        public void DuplicateReferenceNumberTest()
        {
            // ReSharper disable once CollectionNeverQueried.Local
            var duplicateCheck = new Dictionary<string, string>();
            for (var i = 0; i < 1000; i++)
            {
                var result = ReferenceNumberGenerator.IssueTransactionReferenceNumber();
                Console.WriteLine(result);
                Assert.IsNotNull(result);
                duplicateCheck.Add(result, result);
            }
        }
    }
}