using System;
using System.Collections.Generic;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Account;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class StatementModelTestData
    {
        private static readonly ChequeAccount ChequeAccount = new ChequeAccount("Cheque");
        private static readonly VisaAccount VisaAccount = new VisaAccount("Visa");
        private static readonly SpentMonthlyExpense PowerBucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Power");
        private static readonly SpentMonthlyExpense PhoneBucket = new SpentMonthlyExpense(TestDataConstants.PhoneBucketCode, "Phone");
        private static readonly SpentMonthlyExpense HairBucket = new SpentMonthlyExpense(TestDataConstants.HairBucketCode, "Haircuts");
        private static readonly SpentMonthlyExpense RegoBucket = new SpentMonthlyExpense(TestDataConstants.RegoBucketCode, "Car registratios");
        private static readonly SpentMonthlyExpense CarMtcBucket = new SpentMonthlyExpense(TestDataConstants.CarMtcBucketCode, "Car Maintenance");
        private static readonly NamedTransaction TransactionType = new NamedTransaction("Bill Payment");

        /// <summary>
        /// Statement Model with transactions between 15/07/2013 and 14/09/2014
        /// </summary>
        public static StatementModel TestData1()
        {
            var statement = new StatementModel
            {
                FileName = @"C:\TestData1\FooStatement.csv",
                Imported = new DateTime(2013, 08, 15),
            };

            var transactions = CreateTransactions1();
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
    }
}
