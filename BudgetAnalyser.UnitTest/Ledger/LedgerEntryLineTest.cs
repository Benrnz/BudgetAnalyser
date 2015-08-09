using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.UnitTest.Ledger
{
    [TestClass]
    public class LedgerEntryLineTest
    {
        private LedgerEntryLine Subject { get; set; }

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
        }

        private LedgerEntryLine CreateSubject()
        {
            return LedgerBookTestData.TestData5().Reconciliations.First();
        }
    }
}