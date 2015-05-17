using System;
using System.IO;
using BudgetAnalyser.Engine;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.ApplicationHooks
{
    [TestClass]
    public class BatchFileApplicationHookSubscriberTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CtorShouldThrowGivenNullLogger()
        {
            new BatchFileApplicationHookSubscriber(null);
            Assert.Fail();
        }

        [TestMethod]
        public void FileNameShouldReturnRunFolderGivenSetterHasntBeenSet()
        {
            BatchFileApplicationHookSubscriberTestHarness subject = Arrange();
            string result = subject.PrivateFileName;

            Assert.IsTrue(Directory.Exists(Path.GetDirectoryName(result)));
        }

        private BatchFileApplicationHookSubscriberTestHarness Arrange()
        {
            return new BatchFileApplicationHookSubscriberTestHarness();
        }
    }
}