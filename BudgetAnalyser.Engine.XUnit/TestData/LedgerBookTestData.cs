using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.Engine.XUnit.TestData;

public static class LedgerBookTestData
{
    static LedgerBookTestData()
    {
        ChequeAccount = StatementModelTestData.ChequeAccount;
        SavingsAccount = StatementModelTestData.SavingsAccount;
        RatesLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RatesBucketCode, "Rates "), StoredInAccount = ChequeAccount };
        PowerLedger = new SpentPerPeriodLedger { BudgetBucket = new SpentPerPeriodExpenseBucket(TestDataConstants.PowerBucketCode, "Power "), StoredInAccount = ChequeAccount };
        PhoneLedger = new SpentPerPeriodLedger { BudgetBucket = new SpentPerPeriodExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
        WaterLedger = new SpentPerPeriodLedger { BudgetBucket = new SpentPerPeriodExpenseBucket(TestDataConstants.WaterBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
        HouseInsLedger = new SpentPerPeriodLedger { BudgetBucket = new SpentPerPeriodExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
        HouseInsLedgerSavingsAccount = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Poo bar"), StoredInAccount = SavingsAccount };
        CarInsLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.InsuranceCarBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
        LifeInsLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket("INSLIFE", "Poo bar"), StoredInAccount = ChequeAccount };
        CarMtcLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
        RegoLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RegoBucketCode, ""), StoredInAccount = ChequeAccount };
        HairLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow."), StoredInAccount = ChequeAccount };
        ClothesLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.ClothesBucketCode, ""), StoredInAccount = ChequeAccount };
        DocLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.DoctorBucketCode, ""), StoredInAccount = ChequeAccount };
        SurplusLedger = new SurplusLedger { StoredInAccount = ChequeAccount, BudgetBucket = StatementModelTestData.SurplusBucket };
        SavingsSurplusLedger = new SurplusLedger { StoredInAccount = SavingsAccount, BudgetBucket = StatementModelTestData.SurplusBucket };
        SavingsLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.SavingsBucketCode, "Savings"), StoredInAccount = ChequeAccount };
    }

    public static LedgerBucket CarInsLedger { get; }
    public static LedgerBucket CarMtcLedger { get; }

    public static Account ChequeAccount { get; }
    public static LedgerBucket ClothesLedger { get; }
    public static LedgerBucket DocLedger { get; }
    public static LedgerBucket HairLedger { get; }
    public static LedgerBucket HouseInsLedger { get; }

    public static LedgerBucket HouseInsLedgerSavingsAccount { get; }
    public static LedgerBucket LifeInsLedger { get; }
    public static LedgerBucket PhoneLedger { get; }
    public static LedgerBucket PowerLedger { get; }
    public static LedgerBucket RatesLedger { get; }
    public static LedgerBucket RegoLedger { get; }
    public static Account SavingsAccount { get; }
    public static LedgerBucket SavingsLedger { get; }
    public static LedgerBucket SavingsSurplusLedger { get; }
    public static LedgerBucket SurplusLedger { get; }
    public static LedgerBucket WaterLedger { get; }

    /// <summary>
    ///     A Test LedgerBook with data populated for June, July and August 2013.  Also includes some debit transactions.
    /// </summary>
    public static LedgerBook TestData1()
    {
        var line = CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum");
        SetEntriesForTesting(
            line,
            new List<LedgerEntry>
            {
                CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                    }),
                CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                    }),
                CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                    })
            });


        var list = new List<LedgerEntryLine> { line };

        var previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        var previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

        list.Add(
            CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntriesForTesting(
                new List<LedgerEntry>
                {
                    CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                    CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                        }),
                    CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                        })
                }));

        previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

        list.Add(
            CreateLine(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
                "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
                new List<LedgerEntry>
                {
                    CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                    CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                        }),
                    CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                        })
                }));

        var book = new LedgerBook(list) { Name = "Test Data 1 Book", Modified = new DateTime(2013, 12, 16), StorageKey = "C:\\Folder\\book1.xml" };

        Finalise(book);
        return book;
    }

    /// <summary>
    ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
    ///     August transactions include some balance adjustments.
    ///     THIS SET OF TEST DATA IS SUPPOSED TO BE THE SAME AS THE EMBEDDED RESOURCE XML FILE:
    ///     LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml
    /// </summary>
    public static LedgerBook TestData2()
    {
        var list = new List<LedgerEntryLine>
        {
            CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum").SetEntriesForTesting(
                new List
                    <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                            })
                    })
        };

        var previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        var previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

        list.Add(
            CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntriesForTesting(
                new List
                    <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                            })
                    }));

        previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

        var line = CreateLine(
            new DateTime(2013, 08, 15),
            new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
            "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
            new List<LedgerEntry>
            {
                CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                    }),
                CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                    })
            });
        line.BalanceAdjustment(-550, "Credit card payment yet to go out.", StatementModelTestData.ChequeAccount);
        list.Add(line);

        var book = new LedgerBook(list) { Name = "Test Data 2 Book", Modified = new DateTime(2013, 12, 16), StorageKey = "C:\\Folder\\book1.xml" };

        Finalise(book);
        return book;
    }

    /// <summary>
    ///     A Test LedgerBook with data populated for November 2013, last date 15/11/13.
    ///     This was used to seed the actual ledger I use for the first time.
    /// </summary>
    public static LedgerBook TestData3()
    {
        var list = new List<LedgerEntryLine>
        {
            CreateLine(new DateTime(2013, 11, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 10738) }, "Opening entries").SetEntriesForTesting(
                new List
                    <LedgerEntry>
                    {
                        CreateLedgerEntry(RatesLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 573M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 200M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 215M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(WaterLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 50M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(HouseInsLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 100M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(CarInsLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 421M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(LifeInsLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 1626M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(CarMtcLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 163M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(RegoLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 434.73M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 105M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(ClothesLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 403.56M, Narrative = "Opening ledger balance" } }),
                        CreateLedgerEntry(DocLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new CreditLedgerTransaction { Amount = 292.41M, Narrative = "Opening ledger balance" } })
                    })
        };

        var book = new LedgerBook(list) { Name = "Smith Budget 2014", Modified = new DateTime(2013, 12, 22), StorageKey = @"C:\Foo\SmithLedger2014.xml" };

        Finalise(book);
        return book;
    }

    /// <summary>
    ///     Same as Test Data 2, but with multiple Bank Balances for the latest entry.
    ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
    ///     August transactions include some balance adjustments.
    /// </summary>
    public static LedgerBook TestData4()
    {
        var list = new List<LedgerEntryLine>
        {
            CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum").SetEntriesForTesting(
                new List
                    <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                            })
                    })
        };

        var previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        var previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

        list.Add(
            CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntriesForTesting(
                new List
                    <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                            })
                    }));

        previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

        var line = CreateLine(
            new DateTime(2013, 08, 15),
            new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2750), new BankBalance(StatementModelTestData.SavingsAccount, 200) },
            "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
            new List<LedgerEntry>
            {
                CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                    }),
                CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                    })
            });
        line.BalanceAdjustment(-550, "Credit card payment yet to go out.", StatementModelTestData.ChequeAccount);
        list.Add(line);

        var book = new LedgerBook(list) { Name = "Test Data 4 Book", Modified = new DateTime(2013, 12, 16), StorageKey = "C:\\Folder\\book1.xml" };

        Finalise(book);
        return book;
    }

    /// <summary>
    ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
    ///     There are multiple Bank Balances for the latest entry, and the Home Insurance bucket in a different account.
    /// </summary>
    public static LedgerBook TestData5(Func<IEnumerable<LedgerEntryLine>, LedgerBook> ctor = null)
    {
        var list = new List<LedgerEntryLine>
        {
            CreateLine(
                    new DateTime(2013, 06, 15),
                    new[] { new BankBalance(ChequeAccount, 2800), new BankBalance(SavingsAccount, 300) },
                    "Lorem ipsum")
                .SetEntriesForTesting(
                    new List<LedgerEntry>
                    {
                        CreateLedgerEntry(HouseInsLedgerSavingsAccount).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "IbEMWG7" } }),
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                            })
                    })
        };

        var previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        var previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
        var previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

        list.Add(
            CreateLine(
                new DateTime(2013, 07, 15),
                new[] { new BankBalance(ChequeAccount, 4000), new BankBalance(SavingsAccount, 600) },
                "dolor amet set").SetEntriesForTesting(
                new List<LedgerEntry>
                {
                    CreateLedgerEntry(HouseInsLedgerSavingsAccount, previousInsEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "9+1R06x" } }),
                    CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                    CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                        }),
                    CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                        })
                }));

        previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
        previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

        var line = CreateLine(
            new DateTime(2013, 08, 15),
            new[] { new BankBalance(ChequeAccount, 3050), new BankBalance(SavingsAccount, 1000) },
            "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
            new List<LedgerEntry>
            {
                CreateLedgerEntry(HouseInsLedgerSavingsAccount, previousInsEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "agkT9kC" } }),
                CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                    }),
                CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                    new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                    })
            });
        line.BalanceAdjustment(-550, "Credit card payment yet to go out.", ChequeAccount);
        list.Add(line);

        var book = ctor is not null ? ctor(list) : new LedgerBook(list) { StorageKey = "Test Ledger Book.xaml" };
        book.Name = "Test Data 5 Book";
        book.Modified = new DateTime(2013, 12, 16);
        book.StorageKey = "C:\\Folder\\book5.xml";

        Finalise(book);
        return book;
    }

    /// <summary>
    ///     A Test LedgerBook with data populated for August 2013, data is arranged in Fortnightly periods.
    ///     Also includes some debit transactions. There are multiple Bank Balances for the latest entry, and the Home
    ///     Insurance bucket in a different account.
    /// </summary>
    public static LedgerBook TestData6(Func<IEnumerable<LedgerEntryLine>, LedgerBook> ctor = null)
    {
        var list = new List<LedgerEntryLine>
        {
            CreateLine(
                    new DateTime(2013, 8, 1),
                    new[] { new BankBalance(ChequeAccount, 2800), new BankBalance(SavingsAccount, 300) },
                    "Lorem ipsum")
                .SetEntriesForTesting(
                    new List<LedgerEntry>
                    {
                        CreateLedgerEntry(HouseInsLedgerSavingsAccount).SetTransactionsForTesting(
                            new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "IbEMWG7" } }),
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                            })
                    })
        };

        var previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
        var previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
        var previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
        var previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

        list.Add(
            CreateLine(
                new DateTime(2013, 8, 15),
                new[] { new BankBalance(ChequeAccount, 4000), new BankBalance(SavingsAccount, 600) },
                "dolor amet set").SetEntriesForTesting(
                new List<LedgerEntry>
                {
                    CreateLedgerEntry(HouseInsLedgerSavingsAccount, previousInsEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "9+1R06x" } }),
                    CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction> { new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" } }),
                    CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                        }),
                    CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" }, new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                        })
                }));

        var book = ctor is not null ? ctor(list) : new LedgerBook(list) { StorageKey = "Test Ledger Book.xaml" };
        book.Name = "Test Data 6 Book";
        book.Modified = new DateTime(2013, 08, 15);
        book.StorageKey = "C:\\FakeFolder\\book6.xml";

        Finalise(book);
        return book;
    }

    /// <summary>
    ///     Makes sure that the IsNew property on LedgerBook EntryLines is not set to true, as it will be when they are newly
    ///     created.
    ///     Also ensures the StoredInAccount property for each ledger is set.
    /// </summary>
    internal static void Finalise(LedgerBook book, bool unlock = false)
    {
        if (book.Reconciliations.None())
        {
            return;
        }

        var ledgers = new Dictionary<BudgetBucket, LedgerBucket>();
        foreach (var line in book.Reconciliations)
        {
            if (!unlock)
            {
                PrivateAccessor.SetProperty(line, "IsNew", false);
            }

            foreach (var entry in line.Entries)
            {
                if (!unlock)
                {
                    PrivateAccessor.SetField(entry, "isNew", false);
                }

                if (entry.LedgerBucket.StoredInAccount is null)
                {
                    entry.LedgerBucket.StoredInAccount = StatementModelTestData.ChequeAccount;
                }

                if (!ledgers.ContainsKey(entry.LedgerBucket.BudgetBucket))
                {
                    ledgers.Add(entry.LedgerBucket.BudgetBucket, entry.LedgerBucket);
                }
            }
        }

        book.Ledgers = ledgers.Values;
    }

    internal static LedgerEntryLine SetEntriesForTesting(this LedgerEntryLine line, List<LedgerEntry> entries)
    {
        PrivateAccessor.SetProperty(line, "Entries", entries);
        return line;
    }

    internal static LedgerEntry SetTransactionsForTesting(this LedgerEntry entry, List<LedgerTransaction> transactions)
    {
        PrivateAccessor.SetField(entry, "transactions", transactions);
        var newBalance = entry.Balance + entry.NetAmount;
        entry.Balance = newBalance < 0 ? 0 : newBalance;
        return entry;
    }

    private static LedgerEntry CreateLedgerEntry(LedgerBucket ledger, decimal balance = 0)
    {
        return new LedgerEntry { LedgerBucket = ledger, Balance = balance };
    }

    private static LedgerEntryLine CreateLine(DateTime date, IEnumerable<BankBalance> bankBalances, string remarks)
    {
        var line = new LedgerEntryLine(date, bankBalances) { Remarks = remarks };
        return line;
    }
}
