using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.Helper;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    /// <summary>
    ///     These tests resulted from investigating Issue 83.
    /// </summary>
    [TestClass]
    public class SpentMonthlyLedgerReconciliationBehaviourTest
    {
        /*
        Test Cases:
        ===========
        Opening Balance   Budget   Closing Balance   Action
        1) 0              0        0                 None
        2) 1              0        0                 Add supplement txn
        3) 0              1        0                 Add supplement txn
        4) 1              1        0                 Add supplement txn
        5) 0              0        1                 Add remove excess txn
        6) 1              0        1                 None
        7) 0              1        1                 None
        8) 1              1        1                 None

        1's and 0's only indicate difference of values ie whether values are equal to greater than, or less than.
        0's do not indicate the absence of a record.

        */
        private const decimal OpeningBalance = 125M;
        private DateTime reconciliationDate;
        private LedgerEntry subject;

        [TestMethod]
        public void ShouldSupplementToBudgetAmount_GivenNetDifferenceOfZeroAndClosingBalanceIsLessThanBudgetAmount()
        {
            this.subject.Balance = 100;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 300M, Date = new DateTime(2013, 9, 11) },
                new CreditLedgerTransaction { Amount = -300M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(300M, this.subject.Balance);
        }

        [TestMethod]
        public void ShouldRemoveExcessToZero_GivenNetDifferenceOfZeroAndNoBudgetAmount()
        {
            this.subject.Balance = 100;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = 300M, Date = new DateTime(2013, 9, 11) },
                new CreditLedgerTransaction { Amount = -300M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(0M, this.subject.Balance);
        }

        [TestMethod]
        public void ShouldSupplementToZero_GivenNoBudgetAmountAndClosingBalanceIsLessThanZero()
        {
            this.subject.Balance = 100;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -300M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(0M, this.subject.Balance);
        }

        [TestMethod]
        public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndLessThanBudgetAmount()
        {
            this.subject.Balance = 100;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 201M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -300M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(201M, this.subject.Balance);
        }

        [TestMethod]
        public void StatusQuo_TemporaryTest()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 175M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -75M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(125M, this.subject.Balance);
            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestInitialize]
        public void TestInitialise()
        {
            this.reconciliationDate = new DateTime(2013, 9, 20);

            this.subject = new LedgerEntry(true)
            {
                LedgerBucket = LedgerBookTestData.PowerLedger,
                Balance = OpeningBalance
            };
        }

        #region Should Add Compensating Transaction

        [TestMethod]
        [Description("Test case 5")]
        public void ShouldCreateRemoveExcessTransaction_GivenClosingBalanceIsGreaterThanBudgetAmountAndOpeningBalance()
        {
            this.subject.Balance = 0;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 0M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = 1M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test case 5.1")]
        public void ShouldCreateRemoveExcessTransaction_GivenClosingBalanceIsGreaterThanOpeningBalanceAndNoBudgetAmount()
        {
            this.subject.Balance = 0;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = 1M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(2, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test case 3")]
        public void ShouldCreateSupplementTransaction_GivenClosingBalanceIsLessThanBudgetAmountAndLessThanOrEqualToOpeningBalance()
        {
            this.subject.Balance = 0;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -1M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test case 4")]
        public void ShouldCreateSupplementTransaction_GivenClosingBalanceIsLessThanBudgetAmountAndLessThanOpeningBalance()
        {
            this.subject.Balance = 1;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11), Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -2M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test Case 6.1")]
        public void ShouldAddCompensatingTransaction_GivenOpeningBalanceEqualsClosingBalanceAndNoBudgetAmount()
        {
            this.subject.Balance = 1;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
                new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        #endregion

        #region Should NOT Add Compensating Transaction

        [TestMethod]
        [Description("Test case 2 - no budget amount allocated at all")]
        public void ShouldNotCreateSupplementTransaction_GivenClosingBalanceIsLessThanOpeningBalanceAndNoBudgetAmount()
        {
            this.subject.Balance = 1;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(1, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test case 2.1 - budget amount == closing balance")]
        public void ShouldNotCreateSupplementTransaction_GivenClosingBalanceIsLessThanOpeningBalanceAndBudgetAmountEqualsClosingBalance()
        {
            this.subject.Balance = 1;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 1M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(2, this.subject.Transactions.Count());
        }


        [TestMethod]
        [Description("Test case 8")]
        public void ShouldNotAddCompensatingTransaction_GivenClosingBalanceIsEqualToOpeningBalanceAndBudget()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = OpeningBalance, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -OpeningBalance, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(2, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test case 7")]
        public void zzShouldNotAddCompensatingTransaction_GivenClosingBalanceIsEqualToBudgetAndNoWithdrawals()
        {
            this.subject.Balance = 0;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 1, Date = this.reconciliationDate, Narrative = "Budget Amount" }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(1, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test Case 6")]
        public void ShouldNotAddCompensatingTransaction_GivenOpeningBalanceEqualsClosingBalanceAndTransactionsEqualise()
        {
            this.subject.Balance = 1;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 0, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = 1, Date = new DateTime(2013, 9, 11) },
                new CreditLedgerTransaction { Amount = -1, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(3, this.subject.Transactions.Count());
        }

        [TestMethod]
        [Description("Test case 1")]
        public void ShouldNotAddCompensatingTransaction_GivenOpeningBalanceAndClosingBalanceAndBudgetAreAllZero()
        {
            this.subject.Balance = 0;
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>();
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);
            this.subject.Output();

            Assert.AreEqual(0, this.subject.Transactions.Count());
        }

        #endregion

        #region Test for correct values for 'remove-excess' transactions

        [TestMethod]
        [Description("Test case 5.1 Budget amount > Opening Balance (aka Previous Balance)")]
        public void zzShouldOnlyRemoveExcessUpToBudgetAmount_GivenBudgetAmountIsGreaterThanOpeningBalance()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 175M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -75M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(175M, this.subject.Balance);
        }

        [TestMethod]
        [Description("Test case 5.2 Opening Balance > Budget amount")]
        public void ShouldOnlyRemoveExcessUpToOpeningBalance_GivenOpeningBalanceIsGreaterThanBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 105M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -75M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(125M, this.subject.Balance);
        }

        #endregion

        #region Test for correct values for 'supplement' transactions

        [TestMethod]
        [Description("Test case 2.1 Closing Balance 25 < Opening Balance 125 && Opening Balance 125 > Budget Amount 100. Closing balance cannot be less than budget amount")]
        public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndGreaterThanBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 100M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(OpeningBalance, this.subject.Balance);
        }

        [TestMethod]
        [Description("Test case 4.1 Closing Balance 50 < Opening Balance && Opening Balance == Budget Amount 125. Closing balance cannot be less than budget amount")]
        public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanOpeningBalanceAndEqualToBudgetAmount()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 125M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(OpeningBalance, this.subject.Balance);
        }

        [TestMethod]
        [Description("Test case 3.1 Closing Balance < Budget Amount 200 && Budget Amount 200 > Opening Balance 125. Closing balance cannot be less than budget amount")]
        public void ShouldSupplementUpToBudgetAmount_GivenClosingBalanceIsLessThanBudgetAmountAndBudgetAmountIsGreaterThanOpeningBalance()
        {
            Console.WriteLine($"Opening Balance: {this.subject.Balance:F2}");
            var testInput = new List<LedgerTransaction>
            {
                new BudgetCreditLedgerTransaction { Amount = 200M, Date = this.reconciliationDate, Narrative = "Budget Amount" },
                new CreditLedgerTransaction { Amount = -200M, Date = new DateTime(2013, 9, 11) }
            };
            this.subject.SetTransactionsForReconciliation(testInput, this.reconciliationDate);

            this.subject.Output();

            Assert.AreEqual(200M, this.subject.Balance);
        }

        #endregion
    }
}