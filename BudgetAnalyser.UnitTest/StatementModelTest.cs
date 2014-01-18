using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest
{
    [TestClass]
    public class StatementModelTest
    {
        private readonly AccountType cheque = new ChequeAccount("Cheque");
        private readonly AccountType visa = new VisaAccount("Visa");
        private readonly BudgetBucket surplus = new SurplusBucket();
        private readonly BudgetBucket expense1 = new SavedUpForExpense();
        private readonly BudgetBucket income = new IncomeBudgetBucket();
        private readonly TransactionType transactionType = new NamedTransaction("Something");

        [TestMethod]
        public void ValidateShouldPassWhenNoDuplicates()
        {
            var statement = new StatementModel().LoadTransactions(new List<Transaction> { Transaction1, Transaction2, Transaction3 });
            Assert.IsFalse(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void ValidateShouldFailWhenDuplicates1()
        {
            var statement = new StatementModel().LoadTransactions(new List<Transaction> { Transaction1, Transaction2, Transaction3, Duplicate1 });
            Assert.IsTrue(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void ValidateShouldFailWhenDuplicates2()
        {
            var statement = new StatementModel().LoadTransactions(new List<Transaction> { Transaction1, Transaction2, Duplicate2, Transaction3 });
            Assert.IsTrue(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void ValidateShouldFailWhenDuplicates3()
        {
            var statement = new StatementModel().LoadTransactions(new List<Transaction> { Duplicate3, Transaction1, Transaction2, Transaction3 });
            Assert.IsTrue(statement.ValidateAgainstDuplicates().Any());
        }

        [TestMethod]
        public void PerformanceOfValidateTest()
        {
            //BudgetModel budget;
            //var subject = CreateStatementModel(out budget);
            //var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            //subject.ValidateAgainstDuplicates();
            //stopwatch.Stop();

            //Assert.IsTrue(stopwatch.ElapsedMilliseconds < 180);
            Assert.Inconclusive();
        }

        //private StatementModel CreateStatementModel(out BudgetModel budgetModel)
        //{
        //    var mockBucketRepository = new Mock<IBudgetBucketRepository>();
        //    var budgets = new BudgetModelImporter(mockBucketRepository.Object).LoadBudgetData(@"C:\Development\Brees_Unfuddle\BudgetAnalyser\BudgetAnalyser\bin\Debug\BudgetModel.xml");
        //    budgetModel = budgets.CurrentActiveBudget;
        //    var mockMessageBox = new Mock<IUserMessageBox>();
        //    var statementImporter = new AnzAccountStatementImporterV1(mockMessageBox.Object, mockBucketRepository.Object);
        //    return statementImporter.ImportFromFile(@"C:\Development\Brees_Unfuddle\BudgetAnalyser\TestData\8Months.csv", budgetModel, new ChequeAccount("Cheque"));
        //}

        private Transaction Transaction3
        {
            get
            {
                var txn3 = new Transaction
                {
                    AccountType = this.cheque,
                    Amount = 1000.01M,
                    BudgetBucket = this.income,
                    Date = new DateTime(2013, 1, 1),
                    Description = "Salary",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType,
                };
                return txn3;
            }
        }

        private Transaction Transaction2
        {
            get
            {
                var txn2 = new Transaction
                {
                    AccountType = this.visa,
                    Amount = -99.11M,
                    BudgetBucket = this.expense1,
                    Date = new DateTime(2013, 1, 3),
                    Description = "Foo bar",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType,
                };
                return txn2;
            }
        }

        private Transaction Transaction1
        {
            get
            {
                var txn1 = new Transaction
                {
                    AccountType = this.cheque,
                    Amount = -10.01M,
                    BudgetBucket = this.surplus,
                    Date = new DateTime(2013, 1, 1),
                    Description = "abcdefghi",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType,
                };
                return txn1;
            }
        }

        private Transaction Duplicate3
        {
            get
            {
                var txn3 = new Transaction
                {
                    AccountType = this.cheque,
                    Amount = 1000.01M,
                    BudgetBucket = this.income,
                    Date = new DateTime(2013, 1, 1),
                    Description = "Salary",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType,
                };
                return txn3;
            }
        }

        private Transaction Duplicate2
        {
            get
            {
                var txn2 = new Transaction
                {
                    AccountType = this.visa,
                    Amount = -99.11M,
                    BudgetBucket = this.expense1,
                    Date = new DateTime(2013, 1, 3),
                    Description = "Foo bar",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType,
                };
                return txn2;
            }
        }

        private Transaction Duplicate1
        {
            get
            {
                var txn1 = new Transaction
                {
                    AccountType = this.cheque,
                    Amount = -10.01M,
                    BudgetBucket = this.surplus,
                    Date = new DateTime(2013, 1, 1),
                    Description = "abcdefghi",
                    Reference1 = "1",
                    Reference2 = "2",
                    Reference3 = "3",
                    TransactionType = this.transactionType,
                };
                return txn1;
            }
        }
    }
}
