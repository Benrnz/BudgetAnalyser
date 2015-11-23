using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestData;
using BudgetAnalyser.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class ReconciliationBuilderTest
    {
        private ReconciliationBuilder subject;
        private DateTime reconciliationDate;
        private BudgetModel budgetTestData;
        private StatementModel statementTestData;

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject = new ReconciliationBuilder(new FakeLogger());
            this.reconciliationDate = new DateTime(2013, 9, 20);
            this.budgetTestData = BudgetModelTestData.CreateTestData1();
            this.statementTestData = StatementModelTestData.TestData1();
        }

        [TestMethod]
        public void Test1()
        {
            // Issue 83
            this.subject.CreateNewMonthlyReconciliation(this.reconciliationDate, budgetTestData, statementTestData, new BankBalance(LedgerBookTestData.ChequeAccount, 5000M));
        }
    }
}
