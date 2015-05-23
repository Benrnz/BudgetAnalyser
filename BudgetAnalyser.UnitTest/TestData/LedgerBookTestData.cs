using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.UnitTest.TestHarness;
using Rees.TestUtilities;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class LedgerBookTestData
    {
        /// <summary>
        ///     A Test LedgerBook with data populated for June, July and August 2013.  Also includes some debit transactions.
        /// </summary>
        public static LedgerBook TestData1()
        {
            var hairLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };

            var book = new LedgerBook(new FakeLogger())
            {
                Name = "Test Data 1 Book",
                Modified = new DateTime(2013, 12, 16),
                FileName = "C:\\Folder\\book1.xml",
            };

            LedgerEntryLine line = CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum");
            SetEntries(line, new List<LedgerEntry>
            {
                CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                {
                    new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                    new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" },
                }),
                CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                {
                    new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                    new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" },
                }),
                CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                {
                    new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                    new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" },
                })
            });


            var list = new List<LedgerEntryLine> { line };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 08, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
                    "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                    {
                        CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                        }),
                        CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                            new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" },
                        }),
                        CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                            new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" },
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
            var hairLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };

            var book = new LedgerBook(new FakeLogger())
            {
                Name = "Test Data 2 Book",
                Modified = new DateTime(2013, 12, 16),
                FileName = "C:\\Folder\\book1.xml",
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" },
                    })
                }),
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            LedgerEntryLine line = CreateLine(new DateTime(2013, 08, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
                "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" },
                    })
                });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.");
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
            var ratesLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RatesBucketCode, "Rates ") };
            var powerLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };
            var waterLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.WaterBucketCode, "Poo bar") };
            var houseInsLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Poo bar") };
            var carInsLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket("INSCAR", "Poo bar") };
            var lifeInsLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket("INSLIFE", "Poo bar") };
            var carMtcLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Poo bar") };
            var regoLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RegoBucketCode, "") };
            var hairLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var clothesLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket("CLOTHES", "") };
            var docLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.DoctorBucketCode, "") };

            var book = new LedgerBook(new FakeLogger())
            {
                Name = "Smith Budget 2014",
                Modified = new DateTime(2013, 12, 22),
                FileName = @"C:\Foo\SmithLedger2014.xml",
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(new DateTime(2013, 11, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 10738) }, "Opening entries").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(ratesLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 573M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 200M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 215M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(waterLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 50M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(houseInsLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 100M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(carInsLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 421M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(lifeInsLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 1626M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(carMtcLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 163M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(regoLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 434.73M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 105M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(clothesLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 403.56M,  Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(docLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Amount = 292.41M,  Narrative = "Opening ledger balance" },
                    }),
                }),
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
            var hairLedger = new LedgerBucket { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerBucket { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };

            var book = new LedgerBook(new FakeLogger())
            {
                Name = "Test Data 4 Book",
                Modified = new DateTime(2013, 12, 16),
                FileName = "C:\\Folder\\book1.xml",
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(new DateTime(2013, 06, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2500) }, "Lorem ipsum").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut" },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones" },
                    })
                }),
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            LedgerEntryLine line = CreateLine(new DateTime(2013, 08, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2750), new BankBalance(StatementModelTestData.SavingsAccount, 200) },
                "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount" },
                        new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones" },
                    })
                });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.");
            list.Add(line);

            book.SetReconciliations(list);

            return Finalise(book);
        }

        /// <summary>
        ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        ///     There are multiple Bank Balances for the latest entry, and the Home Insurance bucket in a different account.
        ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        ///     August transactions include some balance adjustments.
        /// </summary>
        public static LedgerBook TestData5()
        {
            var chequeAccount = StatementModelTestData.ChequeAccount;
            var savingsAccount = StatementModelTestData.SavingsAccount;
            var hairLedger = new LedgerBucket
            {
                BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow."),
                StoredInAccount = chequeAccount,
            };
            var powerLedger = new LedgerBucket
            {
                BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power "),
                StoredInAccount = chequeAccount,
            };
            var phoneLedger = new LedgerBucket
            {
                BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar"),
                StoredInAccount = chequeAccount,
            };
            var insLedger = new LedgerBucket
            {
                BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Home insurance"),
                StoredInAccount = savingsAccount,
            };

            var book = new LedgerBook(new FakeLogger())
            {
                Name = "Test Data 5 Book",
                Modified = new DateTime(2013, 12, 16),
                FileName = "C:\\Folder\\book5.xml",
            };

            var list = new List<LedgerEntryLine>
            {
                CreateLine(
                    new DateTime(2013, 06, 15), 
                    new[] { new BankBalance(chequeAccount, 2800), new BankBalance(savingsAccount, 300) }, 
                    "Lorem ipsum")
                    .SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(insLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 300M,  Narrative = "Budgeted amount", AutoMatchingReference = "IbEMWG7" },
                    }),
                    CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount", },
                        new CreditLedgerTransaction { Amount = -45M, Narrative = "Hair cut", },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount", },
                        new CreditLedgerTransaction { Amount = -123.56M, Narrative = "Power bill", },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount", },
                        new CreditLedgerTransaction { Amount = -86.43M, Narrative = "Pay phones", },
                    })
                }),
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
            LedgerEntry previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

            list.Add(
                CreateLine(
                    new DateTime(2013, 07, 15), 
                    new[] { new BankBalance(chequeAccount, 4000), new BankBalance(savingsAccount, 600) }, 
                    "dolor amet set").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(insLedger, previousInsEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 300M,  Narrative = "Budgeted amount", AutoMatchingReference = "9+1R06x" },
                    }),
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount",  },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount",  },
                        new CreditLedgerTransaction { Amount = -145.56M, Narrative = "Power bill",  },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount",  },
                        new CreditLedgerTransaction { Amount = -66.43M, Narrative = "Pay phones",  },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
            previousInsEntry = list.Last().Entries.Single(e => e.LedgerBucket.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

            LedgerEntryLine line = CreateLine(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(chequeAccount, 3050), new BankBalance(savingsAccount, 1000) },
                "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(insLedger, previousInsEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 300M,  Narrative = "Budgeted amount", AutoMatchingReference = "agkT9kC"  },
                    }),
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 55M,  Narrative = "Budgeted amount",  },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 140M,  Narrative = "Budgeted amount",  },
                        new CreditLedgerTransaction { Amount = -98.56M, Narrative = "Power bill",  },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Amount = 95M,  Narrative = "Budgeted amount",  },
                        new CreditLedgerTransaction { Amount = -67.43M, Narrative = "Pay phones",  },
                    })
                });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.").WithAccount(chequeAccount);
            list.Add(line);

            book.SetReconciliations(list);

            return Finalise(book);
        }

        private static LedgerEntry CreateLedgerEntry(LedgerBucket ledger, decimal balance = 0)
        {
            return new LedgerEntry { LedgerBucket = ledger, Balance = balance };
        }

        private static LedgerEntryLine CreateLine(DateTime date, IEnumerable<BankBalance> bankBalances, string remarks)
        {
            var line = new LedgerEntryLine(date, bankBalances, new FakeLogger()) { Remarks = remarks };
            return line;
        }

        /// <summary>
        /// Makes sure that the IsNew property on LedgerBook EntryLines is not set to true, as it will be when they are newly created.
        /// Also ensures the StoredInAccount property for each ledger is set.
        /// </summary>
        private static LedgerBook Finalise(LedgerBook book)
        {
            var ledgers = new Dictionary<BudgetBucket, LedgerBucket>();
            foreach (LedgerEntryLine line in book.Reconciliations)
            {
                PrivateAccessor.SetProperty(line, "IsNew", false);
                foreach (LedgerEntry entry in line.Entries)
                {
                    PrivateAccessor.SetField(entry, "isNew", false);
                    if (entry.LedgerBucket.StoredInAccount == null) entry.LedgerBucket.StoredInAccount = StatementModelTestData.ChequeAccount;
                    if (!ledgers.ContainsKey(entry.LedgerBucket.BudgetBucket)) ledgers.Add(entry.LedgerBucket.BudgetBucket, entry.LedgerBucket);
                }
            }

            book.Ledgers = ledgers.Values;
            return book;
        }

        private static LedgerEntryLine SetEntries(this LedgerEntryLine line, List<LedgerEntry> entries)
        {
            PrivateAccessor.SetProperty(line, "Entries", entries);
            return line;
        }

        private static LedgerEntry SetTransactions(this LedgerEntry entry, List<LedgerTransaction> transactions)
        {
            PrivateAccessor.SetField(entry, "transactions", transactions);
            decimal newBalance = entry.Balance + entry.NetAmount;
            entry.Balance = newBalance < 0 ? 0 : newBalance;
            return entry;
        }
    }
}