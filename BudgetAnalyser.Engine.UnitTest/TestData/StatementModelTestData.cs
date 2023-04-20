using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.UnitTest.TestHarness;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.UnitTest.TestData;

public static class StatementModelTestData
{
    public static readonly SavedUpForExpenseBucket CarMtcBucket = new(TestDataConstants.CarMtcBucketCode, "Car Maintenance");
    public static readonly ChequeAccount ChequeAccount = new(TestDataConstants.ChequeAccountName);
    public static readonly SavedUpForExpenseBucket HairBucket = new(TestDataConstants.HairBucketCode, "Haircuts");
    public static readonly IncomeBudgetBucket IncomeBucket = new(TestDataConstants.IncomeBucketCode, "Salary");
    public static readonly SavedUpForExpenseBucket InsHomeBucket = new(TestDataConstants.InsuranceHomeBucketCode, "Insurance Home");
    public static readonly SpentPerPeriodExpenseBucket PhoneBucket = new(TestDataConstants.PhoneBucketCode, "Phone");
    public static readonly SpentPerPeriodExpenseBucket PowerBucket = new(TestDataConstants.PowerBucketCode, "Power");
    public static readonly SpentPerPeriodExpenseBucket RegoBucket = new(TestDataConstants.RegoBucketCode, "Car registrations");
    public static readonly SavingsCommitmentBucket SavingsBucket = new(TestDataConstants.SavingsBucketCode, "Savings Commitment");
    public static readonly PayCreditCardBucket PayCreditCard = new(TestDataConstants.PayCreditCardBucketCode, "Pay Credit Card");
    public static readonly SurplusBucket SurplusBucket = new();
    public static readonly SavingsAccount SavingsAccount = new(TestDataConstants.SavingsAccountName);
    public static readonly NamedTransaction TransactionType = new("Bill Payment");
    public static readonly VisaAccount VisaAccount = new(TestDataConstants.VisaAccountName);

    /// <summary>
    ///     Statement Model with transactions between 15/07/2013 and 14/09/2013
    /// </summary>
    public static StatementModel TestData1()
    {
        var statement = new StatementModel(new FakeLogger())
        {
            StorageKey = @"C:\TestData1\FooStatement.csv",
            LastImport = new DateTime(2013, 08, 15)
        };

        IEnumerable<Transaction> transactions = CreateTransactions1();
        statement.LoadTransactions(transactions);
        return statement;
    }

    /// <summary>
    ///     Statement Model with transactions between 15/07/2013 and 14/09/2013
    ///     Includes income transactions.
    /// </summary>
    public static StatementModel TestData2()
    {
        var statement = new StatementModel(new FakeLogger())
        {
            StorageKey = @"C:\TestData2\Foo2Statement.csv",
            LastImport = new DateTime(2013, 08, 15)
        };

        IEnumerable<Transaction> transactions = CreateTransactions2();
        statement.LoadTransactions(transactions);
        return statement;
    }

    /// <summary>
    ///     Statement Model with transactions between 15/07/2013 and 14/09/2013
    ///     Includes income transactions.
    ///     Same as TestData2 but with another transaction for PhNet in August.
    /// </summary>
    public static StatementModel TestData2A()
    {
        var statement = new StatementModel(new FakeLogger())
        {
            StorageKey = @"C:\TestData2\Foo2Statement.csv",
            LastImport = new DateTime(2013, 08, 15)
        };

        IList<Transaction> transactions = CreateTransactions2().ToList();
        var modTransaction = transactions.Single(t => t.Date == new DateTime(2013, 07, 16) && t.BudgetBucket == PhoneBucket);
        modTransaction.Date = new DateTime(2013, 08, 16);
        statement.LoadTransactions(transactions);
        return statement;
    }

    /// <summary>
    ///     Statement Model with transactions between 15/07/2013 and 14/09/2013
    ///     Includes income transactions.
    ///     Adjusted for use with LedgerCalculator - No ledgers will be overdrawn when using LedgerBook TestData 1.
    /// </summary>
    public static StatementModel TestData3()
    {
        var statement = new StatementModel(new FakeLogger())
        {
            StorageKey = @"C:\TestData3\Foo2Statement.csv",
            LastImport = new DateTime(2013, 08, 15)
        };

        IEnumerable<Transaction> transactions = CreateTransactions3();
        statement.LoadTransactions(transactions);
        return statement;
    }

    /// <summary>
    ///     Statement Model with transactions between 15/07/2013 and 14/09/2013
    ///     Includes income transactions.
    ///     Adjusted for use with LedgerCalculator - No ledgers will be overdrawn when using LedgerBook TestData 1.
    ///     Includes some duplicate transactions
    /// </summary>
    public static StatementModel TestData4()
    {
        var statement = new StatementModel(new FakeLogger())
        {
            StorageKey = @"C:\TestData4\Foo2Statement.csv",
            LastImport = new DateTime(2013, 08, 15)
        };

        List<Transaction> transactions = CreateTransactions3().ToList();
        transactions.AddRange(CreateTransactions1());
        statement.LoadTransactions(transactions);
        return statement;
    }

    /// <summary>
    ///     Statement Model with transactions between 15/07/2013 and 14/09/2013
    ///     Includes income transactions.
    ///     Adjusted for use with LedgerCalculator - No ledgers will be overdrawn when using LedgerBook TestData 1.
    ///     InsHome transfer transaction move funds into Savings, this transactions should be automatched when used with
    ///     LedgerBookTestData5 and a Reconciliation is performed.
    /// </summary>
    public static StatementModel TestData5()
    {
        var statement = new StatementModel(new FakeLogger())
        {
            StorageKey = @"C:\TestData5\Foo5Statement.csv",
            LastImport = new DateTime(2013, 08, 15)
        };

        IEnumerable<Transaction> transactions = CreateTransactions5();
        statement.LoadTransactions(transactions);
        return statement;
    }

    public static StatementModel WithNullBudgetBuckets(this StatementModel instance)
    {
        foreach (var txn in instance.AllTransactions)
        {
            PrivateAccessor.SetField(txn, "budgetBucket", null);
        }

        return instance;
    }

    private static IEnumerable<Transaction> CreateTransactions1()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Account = ChequeAccount,
                Amount = -95.15M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 07, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -58.19M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 07, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -89.15M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -68.29M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -55.00M,
                BudgetBucket = HairBucket,
                Date = new DateTime(2013, 08, 22),
                Description = "Rodney Wayne",
                Reference1 = "1233411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -91.00M,
                BudgetBucket = CarMtcBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Ford Ellerslie",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -350.00M,
                BudgetBucket = RegoBucket,
                Date = new DateTime(2013, 09, 01),
                Description = "nzpost nzta car regisration",
                Reference1 = "23411222",
                TransactionType = TransactionType
            }
        };
        return transactions;
    }

    private static IEnumerable<Transaction> CreateTransactions2()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 07, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -95.15M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 07, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -58.19M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 07, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -89.15M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 08, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -68.29M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -55.00M,
                BudgetBucket = HairBucket,
                Date = new DateTime(2013, 08, 22),
                Description = "Rodney Wayne",
                Reference1 = "1233411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -91.00M,
                BudgetBucket = CarMtcBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Ford Ellerslie",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -350.00M,
                BudgetBucket = RegoBucket,
                Date = new DateTime(2013, 09, 01),
                Description = "nzpost nzta car regisration",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 09, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            }
        };
        return transactions;
    }

    private static IEnumerable<Transaction> CreateTransactions3()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 07, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -95.15M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 07, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -58.19M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 07, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -52.32M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 08, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -64.71M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -55.00M,
                BudgetBucket = HairBucket,
                Date = new DateTime(2013, 08, 22),
                Description = "Rodney Wayne",
                Reference1 = "1233411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -91.00M,
                BudgetBucket = CarMtcBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Ford Ellerslie",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -350.00M,
                BudgetBucket = RegoBucket,
                Date = new DateTime(2013, 09, 01),
                Description = "nzpost nzta car regisration",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 09, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            }
        };
        return transactions;
    }

    private static IEnumerable<Transaction> CreateTransactions5()
    {
        var transactions = new List<Transaction>
        {
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 07, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -95.15M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 07, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -58.19M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 07, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -52.32M,
                BudgetBucket = PowerBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Engery Online Electricity",
                Reference1 = "12334458989",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 08, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -64.71M,
                BudgetBucket = PhoneBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Vodafone Mobile Ltd",
                Reference1 = "1233411119",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -55.00M,
                BudgetBucket = HairBucket,
                Date = new DateTime(2013, 08, 22),
                Description = "Rodney Wayne",
                Reference1 = "1233411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -91.00M,
                BudgetBucket = CarMtcBucket,
                Date = new DateTime(2013, 08, 15),
                Description = "Ford Ellerslie",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = VisaAccount,
                Amount = -350.00M,
                BudgetBucket = RegoBucket,
                Date = new DateTime(2013, 09, 01),
                Description = "nzpost nzta car regisration",
                Reference1 = "23411222",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = 850.99M,
                BudgetBucket = IncomeBucket,
                Date = new DateTime(2013, 09, 20),
                Description = "Payroll",
                Reference1 = "123xxxxxx89",
                TransactionType = TransactionType
            },
            new()
            {
                Account = ChequeAccount,
                Amount = -300M,
                BudgetBucket = InsHomeBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Transfer InsHome monthly budget amount",
                Reference1 = "agkT9kC",
                Reference2 = InsHomeBucket.Code,
                TransactionType = TransactionType
            },
            new()
            {
                Account = SavingsAccount,
                Amount = 300M,
                BudgetBucket = InsHomeBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Transfer InsHome monthly budget amount",
                Reference1 = "agkT9kC",
                Reference2 = InsHomeBucket.Code,
                TransactionType = TransactionType
            },
            new()
            {
                Account = SavingsAccount,
                Amount = -1000M,
                BudgetBucket = InsHomeBucket,
                Date = new DateTime(2013, 08, 16),
                Description = "Pay annual house insurance",
                Reference1 = "HOUSE1234",
                Reference2 = InsHomeBucket.Code,
                TransactionType = TransactionType
            }
        };
        return transactions;
    }
}