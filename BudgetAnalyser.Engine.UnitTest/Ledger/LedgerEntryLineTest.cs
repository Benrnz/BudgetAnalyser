using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Ledger
{
    [TestClass]
    public class LedgerEntryLineTest
    {
        private static readonly BankBalance NextReconcileBankBalance = new(StatementModelTestData.ChequeAccount, 1850.5M);
        private static readonly DateTime NextReconcileDate = new(2013, 09, 15);

        private LedgerEntryLine Subject { get; set; }

        private LedgerEntryLine NewSubject { get; set; }

        [TestMethod]
        public void UpdateRemarks_ShouldSetRemarks()
        {
            NewSubject.UpdateRemarks("Foo bar");

            Assert.AreEqual("Foo bar", NewSubject.Remarks);
        }

        [TestMethod]
        public void RemoveTransactionShouldGiveSurplus1555()
        {
            // Starting Surplus is $1,530.50
            var entry = NewSubject.Entries.First();
            entry.RemoveTransaction(entry.Transactions.First(t => t is CreditLedgerTransaction).Id);

            // The balance of a ledger cannot simply be calculated inside the ledger line. It must be recalc'ed at the ledger book level.
            Assert.AreEqual(1530.50M, NewSubject.CalculatedSurplus); // It should be unchanged.
        }

        [TestMethod]
        public void AddAdjustment_ShouldAffectLedgerBalance()
        {
            NewSubject.BalanceAdjustment(-101M, "foo dar far", new ChequeAccount("Chq"));

            Assert.AreEqual(1749.50M, NewSubject.LedgerBalance);
        }

        [TestMethod]
        public void AddBalanceAdjustment_ShouldAddToAdjustmentCollection()
        {
            NewSubject.BalanceAdjustment(101M, "foo dar far", new ChequeAccount("Chq"));

            Assert.AreEqual(1, Subject.BankBalanceAdjustments.Count());
        }

        [TestMethod]
        public void Output()
        {
            Console.WriteLine("Date: " + Subject.Date);
            Console.WriteLine("Remarks: " + Subject.Remarks);
            Console.Write("Entries: x{0} (", Subject.Entries.Count());
            foreach (LedgerEntry entry in Subject.Entries)
            {
                Console.Write("{0}, ", entry.LedgerBucket.BudgetBucket.Code);
            }
            Console.WriteLine(")");

            Console.WriteLine("Bank Balances:");
            foreach (BankBalance bankBalance in Subject.BankBalances)
            {
                Console.WriteLine("    {0} {1:N}", bankBalance.Account.Name, bankBalance.Balance);
            }
            Console.WriteLine("    ========================");
            Console.WriteLine("TotalBankBalance: " + Subject.TotalBankBalance);

            Console.WriteLine("Balance Adjustments:");
            foreach (BankBalanceAdjustmentTransaction adjustment in Subject.BankBalanceAdjustments)
            {
                Console.WriteLine("    {0} {1} {2:N}", adjustment.BankAccount.Name, adjustment.Narrative, adjustment.Amount);
            }
            Console.WriteLine("    ========================");
            Console.WriteLine("TotalBalanceAdjustments: " + Subject.TotalBalanceAdjustments);

            Console.WriteLine();
            Console.WriteLine("Ledger Balance: " + Subject.LedgerBalance);

            Console.WriteLine();
            Console.WriteLine("Surplus Balances:");
            foreach (BankBalance surplusBalance in Subject.SurplusBalances)
            {
                Console.WriteLine("    {0} {1:N}", surplusBalance.Account.Name, surplusBalance.Balance);
            }
            Console.WriteLine("    ========================");
            Console.WriteLine("CalculatedSurplus aka Total Surplus: " + Subject.CalculatedSurplus);
        }

        [TestMethod]
        public void SurplusBalancesShouldHave2Items()
        {
            IEnumerable<BankBalance> surplusBalances = Subject.SurplusBalances;
            Assert.AreEqual(2, surplusBalances.Count());
        }

        [TestMethod]
        public void SurplusBalancesShouldHaveSavingsBalanceOf100()
        {
            IEnumerable<BankBalance> surplusBalances = Subject.SurplusBalances;
            Assert.AreEqual(100M, surplusBalances.Single(b => b.Account.Name == TestDataConstants.SavingsAccountName).Balance);
        }

        [TestInitialize]
        public void TestInitialise()
        {
            Subject = CreateSubject();
            NewSubject = new LedgerEntryLine(NextReconcileDate, new[] { NextReconcileBankBalance });
            NewSubject.SetNewLedgerEntries(new List<LedgerEntry>
            {
                new(true) { Balance = 120, LedgerBucket = LedgerBookTestData.ClothesLedger },
                new(true) { Balance = 200, LedgerBucket = LedgerBookTestData.DocLedger }
            });
            NewSubject.Entries.First().SetTransactionsForReconciliation(new List<LedgerTransaction>
            {
                new CreditLedgerTransaction { Amount = -80 },
                new BudgetCreditLedgerTransaction { Amount = 200 },
            });
        }

        private LedgerEntryLine CreateSubject()
        {
            return LedgerBookTestData.TestData5().Reconciliations.First();
        }
    }
}