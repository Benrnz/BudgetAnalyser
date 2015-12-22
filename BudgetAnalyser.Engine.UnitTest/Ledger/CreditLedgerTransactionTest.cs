using System;
using BudgetAnalyser.Engine.Ledger;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class CreditLedgerTransactionTest
    {
        [TestMethod]
        public void ConstructWithGuidShouldSetId()
        {
            Guid id = Guid.NewGuid();
            var subject = new CreditLedgerTransaction(id);

            Assert.AreEqual(id, subject.Id);
        }

        [TestMethod]
        public void WithAmount50ShouldCreateACreditOf50()
        {
            var subject = new CreditLedgerTransaction();

            LedgerTransaction result = subject.WithAmount(50);

            Assert.AreEqual(50M, result.Amount);
        }

        [TestMethod]
        public void WithAmount50ShouldReturnSameObjectForChaining()
        {
            var subject = new CreditLedgerTransaction();

            LedgerTransaction result = subject.WithAmount(50);

            Assert.AreSame(subject, result);
        }

        [TestMethod]
        public void WithAmountMinus50ShouldCreateADebitOf50()
        {
            var subject = new CreditLedgerTransaction();

            LedgerTransaction result = subject.WithAmount(-50);

            Assert.AreEqual(-50M, result.Amount);
        }
    }
}