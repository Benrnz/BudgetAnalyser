using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Reconciliation;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BudgetAnalyser.Engine.UnitTest.Widgets
{
    [TestClass]
    public class OverspentWarningTest
    {
        private IBudgetCurrencyContext BudgetCurrencyContext { get; set; }
        private GlobalFilterCriteria Filter => new GlobalFilterCriteria { BeginDate = new DateTime(2013, 08, 15), EndDate = new DateTime(2013, 09, 14) };
        private IDictionary<BudgetBucket, decimal> LedgerBalancesFake { get; set; }
        private LedgerBook LedgerBook { get; set; }
        private LedgerCalculation LedgerCalculator { get; set; }
        private StatementModel Statement { get; set; }
        private OverspentWarning Subject { get; set; }

        [TestInitialize]
        public void TestInitialise()
        {
            Statement = StatementModelTestData.TestData2();

            // Mocking out the Calculator means we dont need the LedgerBook
            LedgerBook = new LedgerBookTestHarness(new Mock<IReconciliationBuilder>().Object);
            SetLedgerBalancesFakeDataSomeOverspentBuckets();

            Subject = new OverspentWarning();
            Act();
        }

        [TestMethod]
        public void UpdateShouldAdd2ElementsToOverSpentSummaryGivenOverSpentTestData()
        {
            Assert.AreEqual(2, Subject.OverSpentSummary.Count());
        }

        [TestMethod]
        public void UpdateShouldAddHairElementToOverSpentSummaryGivenOverSpentTestData()
        {
            Assert.IsTrue(Subject.OverSpentSummary.Any(s => s.Key == StatementModelTestData.HairBucket));
        }

        [TestMethod]
        public void UpdateShouldAddPhoneElementToOverSpentSummaryGivenOverSpentTestData()
        {
            Assert.IsTrue(Subject.OverSpentSummary.Any(s => s.Key == StatementModelTestData.PhoneBucket));
        }

        [TestMethod]
        public void UpdateShouldSetEnabledToFalseGivenStatementIsNull()
        {
            Statement = null;
            Act();
            Assert.IsFalse(Subject.Enabled);
        }

        [TestMethod]
        public void UpdateShouldSetEnabledToTrueGivenAllDependenciesAreValid()
        {
            Assert.IsTrue(Subject.Enabled);
        }

        [TestMethod]
        public void UpdateShouldSetLargeNumberTo0GivenUnderSpentTestData()
        {
            SetLedgerBalancesFakeDataNoOverSpentBuckets();
            Act();
            Assert.AreEqual("0", Subject.LargeNumber);
        }

        [TestMethod]
        public void UpdateShouldSetLargeNumberTo2GivenOverSpentTestData()
        {
            Assert.AreEqual("2", Subject.LargeNumber);
        }

        [TestMethod]
        public void UpdateShouldSetWarningStyleGivenOverSpentTestData()
        {
            Assert.AreEqual("WidgetWarningStyle", Subject.ColourStyleName);
        }

        [TestMethod]
        public void UpdateShouldSetWarningStyleGivenUnderSpentTestData()
        {
            SetLedgerBalancesFakeDataNoOverSpentBuckets();
            Act();
            Assert.AreEqual("WidgetStandardStyle", Subject.ColourStyleName);
        }

        private void Act()
        {
            var expenseListFake = new List<Expense>
            {
                new Expense { Bucket = StatementModelTestData.CarMtcBucket, Amount = 90 }, // Overspent by $1
                new Expense { Bucket = StatementModelTestData.HairBucket, Amount = 55 },
                new Expense { Bucket = StatementModelTestData.PhoneBucket, Amount = 65 }, // Overpsent 3.29
                new Expense { Bucket = StatementModelTestData.PowerBucket, Amount = 100 },
                new Expense { Bucket = StatementModelTestData.RegoBucket, Amount = 20 }
            };
            var modelMock = new Mock<BudgetModel>();
            modelMock.Setup(m => m.Expenses).Returns(expenseListFake);
            var budgetCurrencyContextMock = new Mock<IBudgetCurrencyContext>();
            budgetCurrencyContextMock.Setup(m => m.Model).Returns(modelMock.Object);
            BudgetCurrencyContext = budgetCurrencyContextMock.Object;

            // Mocking out the Calculator means we dont need the LedgerBook
            var ledgerCalculatorMock = new Mock<LedgerCalculation>();
            ledgerCalculatorMock.Setup(m => m.CalculateCurrentMonthLedgerBalances(It.IsAny<LedgerBook>(), It.IsAny<GlobalFilterCriteria>(), It.IsAny<StatementModel>())).Returns(LedgerBalancesFake);
            LedgerCalculator = ledgerCalculatorMock.Object;

            Subject.Update(Statement, BudgetCurrencyContext, Filter, LedgerBook, LedgerCalculator);
        }

        private void SetLedgerBalancesFakeDataNoOverSpentBuckets()
        {
            LedgerBalancesFake = new Dictionary<BudgetBucket, decimal>
            {
                { StatementModelTestData.CarMtcBucket, 10 },
                { StatementModelTestData.HairBucket, -10.00M },
                { StatementModelTestData.PowerBucket, 0 },
                { StatementModelTestData.RegoBucket, -3 },
                { StatementModelTestData.PhoneBucket, 10 }
            };
        }

        private void SetLedgerBalancesFakeDataSomeOverspentBuckets()
        {
            // Phone is intentionally missing from here, so testing fall back to budget can occur.
            // These are balances remaining after looking up the ledger balance and subtracting all spending transactions.
            LedgerBalancesFake = new Dictionary<BudgetBucket, decimal>
            {
                { StatementModelTestData.CarMtcBucket, 10 },
                { StatementModelTestData.HairBucket, -10.01M },
                { StatementModelTestData.PowerBucket, 0 },
                { StatementModelTestData.RegoBucket, -3 }
            };
        }
    }
}