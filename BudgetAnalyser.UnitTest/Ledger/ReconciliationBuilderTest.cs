using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class ReconciliationBuilderTest
    {
        private ReconciliationBuilder subject;
        private DateTime reconciliationDate;

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new ReconciliationBuilder(new FakeLogger());
            this.reconciliationDate = new DateTime(2015, 10, 20);
        }

        [TestMethod]
        public void Test1()
        {
            //this.subject.CreateNewMonthlyReconciliation(this.reconciliationDate, )
        }
    }
}
