using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestHarness;
using Rees.UnitTestUtilities;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class LedgerBookTestData
    {
        static LedgerBookTestData()
        {
            ChequeAccount = StatementModelTestData.ChequeAccount;
            SavingsAccount = StatementModelTestData.SavingsAccount;
            RatesLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RatesBucketCode, "Rates "), StoredInAccount = ChequeAccount };
            PowerLedger = new SpentMonthlyLedger { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power "), StoredInAccount = ChequeAccount };
            PhoneLedger = new SpentMonthlyLedger { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
            WaterLedger = new SpentMonthlyLedger { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.WaterBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
            HouseInsLedger = new SpentMonthlyLedger { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
            HouseInsLedgerSavingsAccount = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Poo bar"), StoredInAccount = SavingsAccount };
            CarInsLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket("INSCAR", "Poo bar"), StoredInAccount = ChequeAccount };
            LifeInsLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket("INSLIFE", "Poo bar"), StoredInAccount = ChequeAccount };
            CarMtcLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Poo bar"), StoredInAccount = ChequeAccount };
            RegoLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RegoBucketCode, ""), StoredInAccount = ChequeAccount };
            HairLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow."), StoredInAccount = ChequeAccount };
            ClothesLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket("CLOTHES", ""), StoredInAccount = ChequeAccount };
            DocLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.DoctorBucketCode, ""), StoredInAccount = ChequeAccount };
            SurplusLedger = new SurplusLedger { StoredInAccount = ChequeAccount };
            SavingsSurplusLedger = new SurplusLedger { StoredInAccount = SavingsAccount };
            SavingsLedger = new SavedUpForLedger { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.SavingsBucketCode, "Savings"), StoredInAccount = ChequeAccount };
        }

        public static LedgerBucket CarInsLedger { get; }
        public static LedgerBucket CarMtcLedger { get; }

        public static Engine.BankAccount.Account ChequeAccount { get; }
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
        public static Engine.BankAccount.Account SavingsAccount { get; }
        public static LedgerBucket SavingsLedger { get; }
        public static LedgerBucket SurplusLedger { get; }
        public static LedgerBucket SavingsSurplusLedger { get; }
        public static LedgerBucket WaterLedger { get; }

        /// <summary>
        ///     A Test LedgerBook with data populated for June, July and August 2013.  Also includes some debit transactions.
        /// </summary>
        public static LedgerBook TestData1()
        {
            var book = new LedgerBook(new ReconciliationBuilder(new FakeLogger()))
            {
                Name = "Test Data 1 Book",
                Modified = new DateTime(2013, 12, 16),
                StorageKey = "C:\\Folder\\book1.xml"
            };

            LedgerEntryLine line = CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum");
            SetEntriesForTesting(
                line,
                new List<LedgerEntry>
                {
                    CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" },
                            new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                        }),
                    CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                            new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                        }),
                    CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                        new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                            new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                        })
                });


            var list = new List<LedgerEntryLine> { line };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntriesForTesting(
                    new List<LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                            }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
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
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                                }),
                            CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                                }),
                            CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                                })
                        }));

            book.SetReconciliations(list);

            return Finalise(book);
        }

        /// <summary>
        ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        ///     August transactions include some balance adjustments.
        ///     THIS SET OF TEST DATA IS SUPPOSED TO BE THE SAME AS THE EMBEDDED RESOURCE XML FILE:
        ///     LedgerBookRepositoryTest_Load_ShouldLoadTheXmlFile.xml
        /// </summary>
        public static LedgerBook TestData2()
        {
            var book = new LedgerBook(new ReconciliationBuilder(new FakeLogger()))
            {
                Name = "Test Data 2 Book",
                Modified = new DateTime(2013, 12, 16),
                StorageKey = "C:\\Folder\\book1.xml"
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum").SetEntriesForTesting(
                    new List
                        <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                            })
                    })
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntriesForTesting(
                    new List
                        <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                            }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                            })
                    }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            LedgerEntryLine line = CreateLine(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
                "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
                    new List<LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                            }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                            })
                    });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.", StatementModelTestData.ChequeAccount);
            list.Add(line);

            book.SetReconciliations(list);

            return Finalise(book);
        }

        /// <summary>
        ///     A Test LedgerBook with data populated for November 2013, last date 15/11/13.
        ///     This was used to seed the actual ledger I use for the first time.
        /// </summary>
        public static LedgerBook TestData3()
        {
            var book = new LedgerBook(new ReconciliationBuilder(new FakeLogger()))
            {
                Name = "Smith Budget 2014",
                Modified = new DateTime(2013, 12, 22),
                StorageKey = @"C:\Foo\SmithLedger2014.xml"
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(new DateTime(2013, 11, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 10738) }, "Opening entries").SetEntriesForTesting(
                    new List
                        <LedgerEntry>
                    {
                        CreateLedgerEntry(RatesLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 573M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 200M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 215M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(WaterLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 50M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(HouseInsLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 100M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(CarInsLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 421M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(LifeInsLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 1626M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(CarMtcLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 163M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(RegoLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 434.73M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 105M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(ClothesLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 403.56M, Narrative = "Opening ledger balance" }
                            }),
                        CreateLedgerEntry(DocLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new CreditLedgerTransaction { Amount = 292.41M, Narrative = "Opening ledger balance" }
                            })
                    })
            };

            book.SetReconciliations(list);

            return Finalise(book);
        }

        /// <summary>
        ///     Same as Test Data 2, but with multiple Bank Balances for the latest entry.
        ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        ///     August transactions include some balance adjustments.
        /// </summary>
        public static LedgerBook TestData4()
        {
            var book = new LedgerBook(new ReconciliationBuilder(new FakeLogger()))
            {
                Name = "Test Data 4 Book",
                Modified = new DateTime(2013, 12, 16),
                StorageKey = "C:\\Folder\\book1.xml"
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum").SetEntriesForTesting(
                    new List
                        <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                            }),
                        CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                            })
                    })
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntriesForTesting(
                    new List
                        <LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                            }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                            })
                    }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            LedgerEntryLine line = CreateLine(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2750), new BankBalance(StatementModelTestData.SavingsAccount, 200) },
                "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
                    new List<LedgerEntry>
                    {
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                            }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                            })
                    });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.", StatementModelTestData.ChequeAccount);
            list.Add(line);

            book.SetReconciliations(list);

            return Finalise(book);
        }

        /// <summary>
        ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        ///     There are multiple Bank Balances for the latest entry, and the Home Insurance bucket in a different account.
        /// </summary>
        public static LedgerBook TestData5(Func<LedgerBook> ctor = null)
        {
            LedgerBook book;
            if (ctor != null)
            {
                book = ctor();
            }
            else
            {
                book = new LedgerBook(new ReconciliationBuilder(new FakeLogger()));
            }
            book.Name = "Test Data 5 Book";
            book.Modified = new DateTime(2013, 12, 16);
            book.StorageKey = "C:\\Folder\\book5.xml";

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
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "IbEMWG7" }
                                }),
                            CreateLedgerEntry(HairLedger).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" }
                                }),
                            CreateLedgerEntry(PowerLedger).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" }
                                }),
                            CreateLedgerEntry(PhoneLedger).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" }
                                })
                        })
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
            LedgerEntry previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

            list.Add(
                CreateLine(
                    new DateTime(2013, 07, 15),
                    new[] { new BankBalance(ChequeAccount, 4000), new BankBalance(SavingsAccount, 600) },
                    "dolor amet set").SetEntriesForTesting(
                        new List<LedgerEntry>
                        {
                            CreateLedgerEntry(HouseInsLedgerSavingsAccount, previousInsEntry.Balance).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "9+1R06x" }
                                }),
                            CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                                }),
                            CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" }
                                }),
                            CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                                new List<LedgerTransaction>
                                {
                                    new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                    new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" }
                                })
                        }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
            previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

            LedgerEntryLine line = CreateLine(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(ChequeAccount, 3050), new BankBalance(SavingsAccount, 1000) },
                "The quick brown fox jumped over the lazy dog").SetEntriesForTesting(
                    new List<LedgerEntry>
                    {
                        CreateLedgerEntry(HouseInsLedgerSavingsAccount, previousInsEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 300M, Narrative = "Budgeted amount", AutoMatchingReference = "agkT9kC" }
                            }),
                        CreateLedgerEntry(HairLedger, previousHairEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 55M, Narrative = "Budgeted amount" }
                            }),
                        CreateLedgerEntry(PowerLedger, previousPowerEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 140M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" }
                            }),
                        CreateLedgerEntry(PhoneLedger, previousPhoneEntry.Balance).SetTransactionsForTesting(
                            new List<LedgerTransaction>
                            {
                                new BudgetCreditLedgerTransaction { Amount = 95M, Narrative = "Budgeted amount" },
                                new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" }
                            })
                    });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.", ChequeAccount);
            list.Add(line);

            book.SetReconciliations(list);

            return Finalise(book);
        }

        /// <summary>
        ///     Makes sure that the IsNew property on LedgerBook EntryLines is not set to true, as it will be when they are newly
        ///     created.
        ///     Also ensures the StoredInAccount property for each ledger is set.
        /// </summary>
        internal static LedgerBook Finalise(LedgerBook book, bool unlock = false)
        {
            var ledgers = new Dictionary<BudgetBucket, LedgerBucket>();
            foreach (LedgerEntryLine line in book.Reconciliations)
            {
                if (!unlock)
                {
                    PrivateAccessor.SetProperty(line, "IsNew", false);
                }
                foreach (LedgerEntry entry in line.Entries)
                {
                    if (!unlock)
                    {
                        PrivateAccessor.SetField(entry, "isNew", false);
                    }
                    if (entry.LedgerBucket.StoredInAccount == null)
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
            return book;
        }

        internal static LedgerEntryLine SetEntriesForTesting(this LedgerEntryLine line, List<LedgerEntry> entries)
        {
            PrivateAccessor.SetProperty(line, "Entries", entries);
            return line;
        }

        internal static LedgerEntry SetTransactionsForTesting(this LedgerEntry entry, List<LedgerTransaction> transactions)
        {
            PrivateAccessor.SetField(entry, "transactions", transactions);
            decimal newBalance = entry.Balance + entry.NetAmount;
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
}