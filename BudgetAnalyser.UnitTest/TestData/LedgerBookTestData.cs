using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class LedgerBookTestData
    {
        /// <summary>
        /// A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        /// </summary>
        public static LedgerBook TestData1()
        {
            var hairLedger = new Ledger { BudgetBucket = new SavedUpForExpense(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense(TestDataConstants.PhoneBucketCode, "Poo bar") };

            var book = new LedgerBook("Test Data 1 Book", new DateTime(2013, 12, 16), "C:\\Folder\\book1.xml");

            var list = new List<LedgerEntryLine>
            {
                new LedgerEntryLine(new DateTime(2013, 06, 15), 2500, "Lorem ipsum").SetEntries(new List<LedgerEntry>
                {
                    new LedgerEntry(hairLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 45M, Narrative = "Hair cut" },
                    }),
                    new LedgerEntry(powerLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 123.56M, Narrative = "Power bill" },
                    }),
                    new LedgerEntry(phoneLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 86.43M, Narrative = "Pay phones" },
                    })
                }),
            };

            var previousHairEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            var previousPowerEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            var previousPhoneEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                new LedgerEntryLine(new DateTime(2013, 07, 15), 3700, "dolor amet set").SetEntries(new List<LedgerEntry>
                {
                    new LedgerEntry(hairLedger, previousHairEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    new LedgerEntry(powerLedger, previousPowerEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 145.56M, Narrative = "Power bill" },
                    }),
                    new LedgerEntry(phoneLedger, previousPhoneEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
            new LedgerEntryLine(new DateTime(2013, 08, 15), 2950, "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                {
                    new LedgerEntry(hairLedger, previousHairEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    new LedgerEntry(powerLedger, previousPowerEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 98.56M, Narrative = "Power bill" },
                    }),
                    new LedgerEntry(phoneLedger, previousPhoneEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 67.43M, Narrative = "Pay phones" },
                    })
                }));

            book.SetDatedEntries(list);

            return book;
        }

        /// <summary>
        /// A Test LedgerBook with data populated for June July and August 2013.  Also includes some debit transactions.
        /// August transactions include some balance adjustments.
        /// </summary>
        public static LedgerBook TestData2()
        {
            var hairLedger = new Ledger { BudgetBucket = new SavedUpForExpense(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var powerLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense(TestDataConstants.PhoneBucketCode, "Poo bar") };

            var book = new LedgerBook("Test Data 2 Book", new DateTime(2013, 12, 16), "C:\\Folder\\book1.xml");

            var list = new List<LedgerEntryLine>
            {
                new LedgerEntryLine(new DateTime(2013, 06, 15), 2500, "Lorem ipsum").SetEntries(new List<LedgerEntry>
                {
                    new LedgerEntry(hairLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 45M, Narrative = "Hair cut" },
                    }),
                    new LedgerEntry(powerLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 123.56M, Narrative = "Power bill" },
                    }),
                    new LedgerEntry(phoneLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 86.43M, Narrative = "Pay phones" },
                    })
                }),
            };

            var previousHairEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            var previousPowerEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            var previousPhoneEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                new LedgerEntryLine(new DateTime(2013, 07, 15), 3700, "dolor amet set").SetEntries(new List<LedgerEntry>
                {
                    new LedgerEntry(hairLedger, previousHairEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                    }),
                    new LedgerEntry(powerLedger, previousPowerEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 145.56M, Narrative = "Power bill" },
                    }),
                    new LedgerEntry(phoneLedger, previousPhoneEntry).SetTransactions(new List<LedgerTransaction>
                    {
                        new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                        new DebitLedgerTransaction { Credit = 0M, Debit = 66.43M, Narrative = "Pay phones" },
                    })
                }));

            previousHairEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.HairBucketCode);
            previousPowerEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PowerBucketCode);
            previousPhoneEntry = list.Last().Entries.Single(e => e.Ledger.BudgetBucket.Code == TestDataConstants.PhoneBucketCode);

            list.Add(
                new LedgerEntryLine(new DateTime(2013, 08, 15), 2950, "The quick brown fox jumped over the lazy dog").SetEntries(new List<LedgerEntry>
                    {
                        new LedgerEntry(hairLedger, previousHairEntry).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Credit = 55M, Debit = 0M, Narrative = "Budgeted amount" },
                        }),
                        new LedgerEntry(powerLedger, previousPowerEntry).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Credit = 140M, Debit = 0M, Narrative = "Budgeted amount" },
                            new DebitLedgerTransaction { Credit = 0M, Debit = 98.56M, Narrative = "Power bill" },
                        }),
                        new LedgerEntry(phoneLedger, previousPhoneEntry).SetTransactions(new List<LedgerTransaction>
                        {
                            new BudgetCreditLedgerTransaction { Credit = 95M, Debit = 0M, Narrative = "Budgeted amount" },
                            new DebitLedgerTransaction { Credit = 0M, Debit = 67.43M, Narrative = "Pay phones" },
                        })
                    }
                ).SetBalanceAdjustments(new List<LedgerTransaction>
                {
                    new DebitLedgerTransaction { Debit = 550, Narrative = "Credit card payment yet to go out."}
                }));

            book.SetDatedEntries(list);

            return book;
        }

        /// <summary>
        /// A Test LedgerBook with data populated for November 2013, last date 15/11/13.  
        /// This was used to seed the actual ledger I use for the first time.
        /// </summary>
        public static LedgerBook TestData3()
        {
            var ratesLedger = new Ledger { BudgetBucket = new SavedUpForExpense(TestDataConstants.RatesBucketCode, "Rates ") };
            var powerLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense(TestDataConstants.PowerBucketCode, "Power ") };
            var phoneLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense(TestDataConstants.PhoneBucketCode, "Poo bar") };
            var waterLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense("WATER", "Poo bar") };
            var houseInsLedger = new Ledger { BudgetBucket = new SpentMonthlyExpense("INSHOME", "Poo bar") };
            var carInsLedger = new Ledger { BudgetBucket = new SavedUpForExpense("INSCAR", "Poo bar") };
            var lifeInsLedger = new Ledger { BudgetBucket = new SavedUpForExpense("INSLIFE", "Poo bar") };
            var carMtcLedger = new Ledger { BudgetBucket = new SavedUpForExpense(TestDataConstants.CarMtcBucketCode, "Poo bar") };
            var regoLedger = new Ledger { BudgetBucket = new SavedUpForExpense(TestDataConstants.RegoBucketCode, "") };
            var hairLedger = new Ledger { BudgetBucket = new SavedUpForExpense(TestDataConstants.HairBucketCode, "Hair cuts wheelbarrow.") };
            var clothesLedger = new Ledger { BudgetBucket = new SavedUpForExpense("CLOTHES", "") };
            var docLedger = new Ledger { BudgetBucket = new SavedUpForExpense("DOC", "") };

            var book = new LedgerBook("Rees Budget 2014", new DateTime(2013, 12, 22), @"C:\Development\Brees_Unfuddle\Rees Budget Accounts\ReesLedger2014.xml");

            var list = new List<LedgerEntryLine>
            {
                new LedgerEntryLine(new DateTime(2013, 11, 15), 10738, "Opening entries").SetEntries(new List<LedgerEntry>
                {
                    new LedgerEntry(ratesLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 573M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(powerLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 200M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(phoneLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 215M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(waterLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 50M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(houseInsLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 100M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(carInsLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 421M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(lifeInsLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 1626M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(carMtcLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 163M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(regoLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 434.73M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(hairLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 105M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(clothesLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 403.56M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                    new LedgerEntry(docLedger, null).SetTransactions(new List<LedgerTransaction>
                    {
                        new CreditLedgerTransaction { Credit = 292.41M, Debit = 0M, Narrative = "Opening ledger balance" },
                    }),
                }),
            };

            book.SetDatedEntries(list);

            return book;
        }
    }
}
