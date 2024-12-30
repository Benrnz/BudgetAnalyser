﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.Helper;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class SavedUpForLedgerReconciliationBehaviourTest
    {
        private const decimal OpeningBalance = 100M;
        private SavedUpForLedger subject2;
        private readonly DateTime reconciliationDate = new DateTime(2013, 9, 20);

        [TestMethod]
        public void ShouldAddCompensatingTransaction_GivenClosingBalanceLessThanBudgetAmount()
        {
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200, Date = new DateTime(2013, 9, 11) }
            };

            var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

            Assert.IsTrue(result);
            Assert.AreEqual(3, testInput.Count());
        }

        [TestMethod]
        public void ShouldNotAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceGreaterThanZero()
        {
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -20, Date = new DateTime(2013, 9, 11) }
            };
            var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

            Assert.IsFalse(result);
            Assert.AreEqual(1, testInput.Count());
        }

        [TestMethod]
        public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceGreaterThanBudgetAmount()
        {
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -20, Date = new DateTime(2013, 9, 11) }
            };
            var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

            Assert.IsFalse(result);
            Assert.AreEqual(2, testInput.Count());
        }

        [TestMethod]
        public void ShouldAddCompensatingTransaction_GivenNoBudgetAmountAndClosingBalanceLessThanZero()
        {
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -200, Date = new DateTime(2013, 9, 11) }
            };

            var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

            Assert.IsTrue(result);
            Assert.AreEqual(2, testInput.Count);
        }

        [TestMethod]
        public void ShouldSupplementOverdrawnBalance_GivenClosingBalanceLessThanBudgetAmount()
        {
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200, Date = new DateTime(2013, 9, 11) }
            };
            var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

            Assert.IsTrue(result);
            Assert.AreEqual(150, OpeningBalance + testInput.Sum(t => t.Amount));
        }

        [TestMethod]
        public void ShouldNotSupplementPositiveBalance_GivenClosingBalanceGreaterThanBudgetAmount()
        {
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 150, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -20, Date = new DateTime(2013, 9, 11) }
            };

            var result = this.subject2.ApplyReconciliationBehaviour(testInput, this.reconciliationDate, OpeningBalance);

            Assert.IsFalse(result);
            Assert.AreEqual(230M, OpeningBalance + testInput.Sum(t => t.Amount));
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.subject2 = new SavedUpForLedger
            {
                BudgetBucket = StatementModelTestData.CarMtcBucket,
                StoredInAccount = StatementModelTestData.ChequeAccount,
            };
        }
    }
}