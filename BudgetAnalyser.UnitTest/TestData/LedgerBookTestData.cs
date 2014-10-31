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
            var hairLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };

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
                    new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    new DebitLedgerTransaction { Credit = 0M, Debit = 45M, Narrative = "Hair cut" },
                }),
                CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                {
                    new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                    new DebitLedgerTransaction { Credit = 0M, Debit = 123.56M, Narrative = "Power bill" },
                }),
                CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                {
                    new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                    new DebitLedgerTransaction { Credit = 0M, Debit = 86.43M, Narrative = "Pay phones" },
                })
            });


            var list = new List<LedgerEntryLine> { line };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 145.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 08, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
                    "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                    {
                        CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                        }),
                        CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                            new DebitLedgerTransaction { Credit = 0M, Debit = 98.56M, Narrative = "Power bill" },
                        }),
                        CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                            new DebitLedgerTransaction { Credit = 0M, Debit = 67.43M, Narrative = "Pay phones" },
                        })
                    }));

            book.SetDatedEntries(list);

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
            var hairLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };

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
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 45M, Narrative = "Hair cut" },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 123.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 86.43M, Narrative = "Pay phones" },
                    })
                }),
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 145.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            LedgerEntryLine line = CreateLine(new DateTime(2013, 08, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2950) },
                "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 98.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 67.43M, Narrative = "Pay phones" },
                    })
                });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.");
            list.Add(line);

            book.SetDatedEntries(list);

            return Finalise(book);
        }

        /// <summary>
        ///     A Test LedgerBook with data populated for November 2013, last date 15/11/13.
        ///     This was used to seed the actual ledger I use for the first time.
        /// </summary>
        public static LedgerBook TestData3()
        {
            var ratesLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RatesBucketCode, "Rates ") };
            var powerLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };
            var waterLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.WaterBucketCode, "Poo bar") };
            var houseInsLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Poo bar") };
            var carInsLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket("INSCAR", "Poo bar") };
            var lifeInsLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket("INSLIFE", "Poo bar") };
            var carMtcLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.CarMtcBucketCode, "Poo bar") };
            var regoLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.RegoBucketCode, "") };
            var hairLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var clothesLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket("CLOTHES", "") };
            var docLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.DoctorBucketCode, "") };

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
                        new CreditLedgerTransaction { Credit = 573M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 200M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 215M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(waterLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 50M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(houseInsLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 100M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(carInsLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 421M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(lifeInsLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 1626M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(carMtcLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 163M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(regoLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 434.73M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 105M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(clothesLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 403.56M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    CreateLedgerEntry(docLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 292.41M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                }),
            };

            book.SetDatedEntries(list);

            return Finalise(book);
        }

        /// <summary>
        ///     Same as Test Data 2, but with multiple Bank Balances for the latest entry.
        ///     A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        ///     August transactions include some balance adjustments.
        /// </summary>
        public static LedgerBook TestData4()
        {
            var hairLedger = new LedgerColumn { BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new LedgerColumn { BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar") };

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
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 45M, Narrative = "Hair cut" },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 123.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 86.43M, Narrative = "Pay phones" },
                    })
                }),
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                CreateLine(new DateTime(2013, 07, 15), new[] { new BankBalance(StatementModelTestData.ChequeAccount, 3700) }, "dolor amet set").SetEntries(new List
                    <LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 145.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            LedgerEntryLine line = CreateLine(new DateTime(2013, 08, 15),
                new[] { new BankBalance(StatementModelTestData.ChequeAccount, 2750), new BankBalance(StatementModelTestData.SavingsAccount, 200) },
                "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 98.56M, Narrative = "Power bill" },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 67.43M, Narrative = "Pay phones" },
                    })
                });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.");
            list.Add(line);

            book.SetDatedEntries(list);

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
            var hairLedger = new LedgerColumn
            {
                BudgetBucket = new SavedUpForExpenseBucket(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow."),
                StoredInAccount = chequeAccount,
            };
            var powerLedger = new LedgerColumn
            {
                BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PowerBucketCode, "Power "),
                StoredInAccount = chequeAccount,
            };
            var phoneLedger = new LedgerColumn
            {
                BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.PhoneBucketCode, "Poo bar"),
                StoredInAccount = chequeAccount,
            };
            var insLedger = new LedgerColumn
            {
                BudgetBucket = new SpentMonthlyExpenseBucket(TestDataConstants.InsuranceHomeBucketCode, "Home insurance"),
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
                        new BudgetCreditLedgerTransaction { Credit = 300M, Debit = 0M, Narrative = "Budgeted amount", },
                    }),
                    CreateLedgerEntry(hairLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount", },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 45M, Narrative = "Hair cut", },
                    }),
                    CreateLedgerEntry(powerLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount", },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 123.56M, Narrative = "Power bill", },
                    }),
                    CreateLedgerEntry(phoneLedger).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount", },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 86.43M, Narrative = "Pay phones", },
                    })
                }),
            };

            LedgerEntry previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            LedgerEntry previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            LedgerEntry previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
            LedgerEntry previousInsEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

            list.Add(
                CreateLine(
                    new DateTime(2013, 07, 15), 
                    new[] { new BankBalance(chequeAccount, 4000), new BankBalance(savingsAccount, 600) }, 
                    "dolor amet set").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(insLedger, previousInsEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 300M, Debit = 0M, Narrative = "Budgeted amount",  },
                    }),
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount",  },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount",  },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 145.56M, Narrative = "Power bill",  },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount",  },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 66.43M, Narrative = "Pay phones",  },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);
            previousInsEntry = list.Last().Entries.Single(e => e.LedgerColumn.BudgetBucket.Code == TestDataConstants.InsuranceHomeBucketCode);

            LedgerEntryLine line = CreateLine(
                new DateTime(2013, 08, 15),
                new[] { new BankBalance(chequeAccount, 3050), new BankBalance(savingsAccount, 1000) },
                "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    CreateLedgerEntry(insLedger, previousInsEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 300M, Debit = 0M, Narrative = "Budgeted amount",  },
                    }),
                    CreateLedgerEntry(hairLedger, previousHairEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount",  },
                    }),
                    CreateLedgerEntry(powerLedger, previousPowerEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount",  },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 98.56M, Narrative = "Power bill",  },
                    }),
                    CreateLedgerEntry(phoneLedger, previousPhoneEntry.Balance).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount",  },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 67.43M, Narrative = "Pay phones",  },
                    })
                });
            line.BalanceAdjustment(-550, "Credit card payment yet to go out.").WithAccountType(chequeAccount);
            list.Add(line);

            book.SetDatedEntries(list);

            return Finalise(book);
        }

        private static LedgerEntry CreateLedgerEntry(LedgerColumn ledger, decimal balance = 0)
        {
            return new LedgerEntry { LedgerColumn = ledger, Balance = balance };
        }

        private static LedgerEntryLine CreateLine(DateTime date, IEnumerable<BankBalance> bankBalances, string remarks)
        {
            var line = new LedgerEntryLine(date, bankBalances) { Remarks = remarks };
            return line;
        }

        private static LedgerBook Finalise(LedgerBook book)
        {
            var ledgers = new Dictionary<BudgetBucket, LedgerColumn>();
            foreach (LedgerEntryLine line in book.DatedEntries)
            {
                PrivateAccessor.SetProperty(line, "IsNew", false);
                foreach (LedgerEntry entry in line.Entries)
                {
                    PrivateAccessor.SetField(entry, "isNew", false);
                    if (entry.LedgerColumn.StoredInAccount == null) entry.LedgerColumn.StoredInAccount = StatementModelTestData.ChequeAccount;
                    if (!ledgers.ContainsKey(entry.LedgerColumn.BudgetBucket)) ledgers.Add(entry.LedgerColumn.BudgetBucket, entry.LedgerColumn);
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
            PrivateAccessor.SetProperty(entry, "Transactions", transactions);
            decimal newBalance = entry.Balance + entry.NetAmount;
            entry.Balance = newBalance < 0 ? 0 : newBalance;
            return entry;
        }
    }
}