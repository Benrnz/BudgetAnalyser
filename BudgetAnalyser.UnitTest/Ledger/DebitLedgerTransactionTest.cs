using System;
using BudgetAnalyser.Engine.Ledger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class DebitLedgerTransactionTest
    {
        [TestMethod]
        public void ConstructWithGuidShouldSetId()
        {
            Guid id = Guid.NewGuid();
            var subject = new DebitLedgerTransaction(id);

            Assert.AreEqual(id, subject.Id);
        }

        [TestMethod]
        public void WithAmount50ShouldAZeroCreditAmount()
        {
            var subject = new DebitLedgerTransaction();

            LedgerTransaction result = subject.WithAmount(50);

            Assert.AreEqual(0M, result.Credit);
        }

        [TestMethod]
        public void WithAmount50ShouldCreateADebitOf50()
        {
            var subject = new DebitLedgerTransaction();

            LedgerTransaction result = subject.WithAmount(50);

            Assert.AreEqual(50M, result.Debit);
        }

        [TestMethod]
        public void WithAmount50ShouldReturnSameObjectForChaining()
        {
            var subject = new DebitLedgerTransaction();

            LedgerTransaction result = subject.WithAmount(50);

            Assert.AreSame(subject, result);
        }

        [TestMethod]
        public void WithReversal50ShouldAZeroCreditAmount()
        {
            var subject = new DebitLedgerTransaction();

            LedgerTransaction result = subject.WithReversal(50);

            Assert.AreEqual(0M, result.Credit);
        }

        [TestMethod]
        public void WithReversal50ShouldCreateANegativeDebitOf50()
        {
            var subject = new DebitLedgerTransaction();

            LedgerTransaction result = subject.WithReversal(50);

            Assert.AreEqual(-50M, result.Debit);
        }

        [TestMethod]
        public void WithReversal50ShouldReturnSameObjectForChaining()
        {
            var subject = new DebitLedgerTransaction();

            LedgerTransaction result = subject.WithReversal(50);

            Assert.AreSame(subject, result);
        }
    }
}