using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class StatementModelTestData
    {
        public static readonly ChequeAccount ChequeAccount = new ChequeAccount("CHEQUE");
        public static readonly VisaAccount VisaAccount = new VisaAccount("VISA");
        public static readonly SavingsAccount SavingsAccount = new SavingsAccount("SAVINGS");

        public static readonly SpentMonthlyExpenseBucket PowerBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power");
        public static readonly SpentMonthlyExpenseBucket PhoneBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Phone");
        public static readonly SpentMonthlyExpenseBucket HairBucket = new SpentMonthlyExpenseBucket(TestDataConstants.HairBucketCode, "Haircuts");
        public static readonly SpentMonthlyExpenseBucket RegoBucket = new SpentMonthlyExpenseBucket(TestDataConstants.RegoBucketCode, "Car registrations");
        public static readonly SpentMonthlyExpenseBucket CarMtcBucket = new SpentMonthlyExpenseBucket(TestDataConstants.CarMtcBucketCode, "Car Maintenance");
        public static readonly NamedTransaction TransactionType = new NamedTransaction("Bill Payment");
        public static readonly IncomeBudgetBucket IncomeBucket = new IncomeBudgetBucket(TestDataConstants.IncomeBucketCode, "Salary");

        /// <summary>
        /// Statement Model with transactions between 15/07/2013 and 14/09/2014
        /// </summary>
        public static StatementModel TestData1()
        {
            var statement = new StatementModel(new FakeLogger())
            {
                FileName = @"C:\TestData1\FooStatement.csv",
                LastImport = new DateTime(2013, 08, 15),
            };

            var transactions = CreateTransactions1();
            statement.LoadTransactions(transactions);
            return statement;
        }

        /// <summary>
        /// Statement Model with transactions between 15/07/2013 and 14/09/2013
        /// Includes income transactions.
        /// </summary>
        public static StatementModel TestData2()
        {
            var statement = new StatementModel(new FakeLogger())
            {
                FileName = @"C:\TestData2\Foo2Statement.csv",
                LastImport = new DateTime(2013, 08, 15),
            };

            var transactions = CreateTransactions2();
            statement.LoadTransactions(transactions);
            return statement;
        }

        private static IEnumerable<Transaction> CreateTransactions1()
        {
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -95.15M,
                    BudgetBucket = PowerBucket,
                    Date = new DateTime(2013, 07, 15),
                    Description = "Engery Online Electricity",
                    Reference1 = "12334458989",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -58.19M,
                    BudgetBucket = PhoneBucket,
                    Date = new DateTime(2013, 07, 16),
                    Description = "Vodafone Mobile Ltd",
                    Reference1 = "1233411119",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -89.15M,
                    BudgetBucket = PowerBucket,
                    Date = new DateTime(2013, 08, 15),
                    Description = "Engery Online Electricity",
                    Reference1 = "12334458989",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -68.29M,
                    BudgetBucket = PhoneBucket,
                    Date = new DateTime(2013, 08, 16),
                    Description = "Vodafone Mobile Ltd",
                    Reference1 = "1233411119",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = VisaAccount,
                    Amount = -55.00M,
                    BudgetBucket = HairBucket,
                    Date = new DateTime(2013, 08, 22),
                    Description = "Rodney Wayne",
                    Reference1 = "1233411222",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = VisaAccount,
                    Amount = -91.00M,
                    BudgetBucket = CarMtcBucket,
                    Date = new DateTime(2013, 08, 15),
                    Description = "Ford Ellerslie",
                    Reference1 = "23411222",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = VisaAccount,
                    Amount = -350.00M,
                    BudgetBucket = RegoBucket,
                    Date = new DateTime(2013, 09, 01),
                    Description = "nzpost nzta car regisration",
                    Reference1 = "23411222",
                    TransactionType = TransactionType,
                },
            };
            return transactions;
        }

        private static IEnumerable<Transaction> CreateTransactions2()
        {
            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = 850.99M,
                    BudgetBucket = IncomeBucket,
                    Date = new DateTime(2013, 07, 20),
                    Description = "Payroll",
                    Reference1 = "123xxxxxx89",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -95.15M,
                    BudgetBucket = PowerBucket,
                    Date = new DateTime(2013, 07, 15),
                    Description = "Engery Online Electricity",
                    Reference1 = "12334458989",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -58.19M,
                    BudgetBucket = PhoneBucket,
                    Date = new DateTime(2013, 07, 16),
                    Description = "Vodafone Mobile Ltd",
                    Reference1 = "1233411119",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -89.15M,
                    BudgetBucket = PowerBucket,
                    Date = new DateTime(2013, 08, 15),
                    Description = "Engery Online Electricity",
                    Reference1 = "12334458989",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = 850.99M,
                    BudgetBucket = IncomeBucket,
                    Date = new DateTime(2013, 08, 20),
                    Description = "Payroll",
                    Reference1 = "123xxxxxx89",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = -68.29M,
                    BudgetBucket = PhoneBucket,
                    Date = new DateTime(2013, 08, 16),
                    Description = "Vodafone Mobile Ltd",
                    Reference1 = "1233411119",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = VisaAccount,
                    Amount = -55.00M,
                    BudgetBucket = HairBucket,
                    Date = new DateTime(2013, 08, 22),
                    Description = "Rodney Wayne",
                    Reference1 = "1233411222",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = VisaAccount,
                    Amount = -91.00M,
                    BudgetBucket = CarMtcBucket,
                    Date = new DateTime(2013, 08, 15),
                    Description = "Ford Ellerslie",
                    Reference1 = "23411222",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = VisaAccount,
                    Amount = -350.00M,
                    BudgetBucket = RegoBucket,
                    Date = new DateTime(2013, 09, 01),
                    Description = "nzpost nzta car regisration",
                    Reference1 = "23411222",
                    TransactionType = TransactionType,
                },
                new Transaction
                {
                    AccountType = ChequeAccount,
                    Amount = 850.99M,
                    BudgetBucket = IncomeBucket,
                    Date = new DateTime(2013, 09, 20),
                    Description = "Payroll",
                    Reference1 = "123xxxxxx89",
                    TransactionType = TransactionType,
                },
            };
            return transactions;
        }
    }
}
