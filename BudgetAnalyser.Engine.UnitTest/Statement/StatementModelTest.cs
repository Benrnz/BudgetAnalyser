﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestData;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BudgetAnalyser.Engine.UnitTest.Statement
{
    [TestClass]
    public class StatementModelTest
    {
        private readonly Engine.BankAccount.Account cheque = new ChequeAccount("Cheque");
        private readonly BudgetBucket expense1 = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, string.Empty);
        private readonly BudgetBucket income = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, string.Empty);
        private readonly BudgetBucket surplus = new SurplusBucket();
        private readonly TransactionType transactionType = new NamedTransaction("Something");
        private readonly Engine.BankAccount.Account visa = new VisaAccount("Visa");

        private Transaction Duplicate1
        {
            get
            {
                var txn1 = new Transaction
                {
                    Account = this.cheque,
                    Amount = -10.01M,
                    BudgetBucket = this.surplus,
                    Date = new DateTime(2013, 1, 1),
                    Description = "abcdefghi",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType
                };
                return txn1;
            }
        }

        private Transaction Duplicate2
        {
            get
            {
                var txn2 = new Transaction
                {
                    Account = this.visa,
                    Amount = -99.11M,
                    BudgetBucket = this.expense1,
                    Date = new DateTime(2013, 1, 3),
                    Description = "Foo bar",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType
                };
                return txn2;
            }
        }

        private Transaction Duplicate3
        {
            get
            {
                var txn3 = new Transaction
                {
                    Account = this.cheque,
                    Amount = 1000.01M,
                    BudgetBucket = this.income,
                    Date = new DateTime(2013, 1, 1),
                    Description = "Salary",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType
                };
                return txn3;
            }
        }

        private Transaction Transaction1
        {
            get
            {
                var txn1 = new Transaction
                {
                    Account = this.cheque,
                    Amount = -10.01M,
                    BudgetBucket = this.surplus,
                    Date = new DateTime(2013, 1, 1),
                    Description = "abcdefghi",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType
                };
                return txn1;
            }
        }

        private Transaction Transaction2
        {
            get
            {
                var txn2 = new Transaction
                {
                    Account = this.visa,
                    Amount = -99.11M,
                    BudgetBucket = this.expense1,
                    Date = new DateTime(2013, 1, 3),
                    Description = "Foo bar",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType
                };
                return txn2;
            }
        }

        private Transaction Transaction3
        {
            get
            {
                var txn3 = new Transaction
                {
                    Account = this.cheque,
                    Amount = 1000.01M,
                    BudgetBucket = this.income,
                    Date = new DateTime(2013, 1, 1),
                    Description = "Salary",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType
                };
                return txn3;
            }
        }

        [TestMethod]
        public void PerformanceOfValidateTest()
        {
            StatementModel subject = StatementModelTestData.TestData1();
            Stopwatch stopwatch = Stopwatch.StartNew();
            subject.ValidateAgainstDuplicates();
            stopwatch.Stop();
            Console.WriteLine("{0:N0} ms", stopwatch.ElapsedMilliseconds);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 250);
        }

        [TestMethod]
        public void ValidateShouldFailWhenDuplicates1()
        {
            StatementModel statement = new StatementModel(new FakeLogger()).LoadTransactions(new List<Transaction> { Transaction1, Transaction2, Transaction3, Duplicate1 });
            Assert.IsTrue(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void ValidateShouldFailWhenDuplicates2()
        {
            StatementModel statement = new StatementModel(new FakeLogger()).LoadTransactions(new List<Transaction> { Transaction1, Transaction2, Duplicate2, Transaction3 });
            Assert.IsTrue(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void ValidateShouldFailWhenDuplicates3()
        {
            StatementModel statement = new StatementModel(new FakeLogger()).LoadTransactions(new List<Transaction> { Duplicate3, Transaction1, Transaction2, Transaction3 });
            Assert.IsTrue(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void ValidateShouldPassWhenNoDuplicates()
        {
            StatementModel statement = new StatementModel(new FakeLogger()).LoadTransactions(new List<Transaction> { Transaction1, Transaction2, Transaction3 });
            Assert.IsFalse(statement.ValidateAgainstDuplicates().Any());
        }
    }
}