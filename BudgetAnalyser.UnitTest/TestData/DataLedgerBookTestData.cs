﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class DataLedgerBookTestData
    {
        private static readonly Guid id1 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A0");
        private static readonly Guid id2 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A1");
        private static readonly Guid id3 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A2");
        private static readonly Guid id4 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A3");
        private static readonly Guid id5 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A4");
        private static readonly Guid id6 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A5");
        private static readonly Guid id7 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A6");
        private static readonly Guid id8 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A7");
        private static readonly Guid id9 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A8");
        private static readonly Guid id10 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A9");
        private static readonly Guid id11 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6AA");

        public static DataLedgerBook TestData1()
        {
            var book = new DataLedgerBook
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 1",
                FileName = "C:\\Folder\\FooBook.xml",
            };

            var lines = new List<DataLedgerEntryLine>();

            var line1 = AddEntryLineForTestData1(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(new[]
            {
                new DataLedgerEntry
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id1,
                            Credit = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new DataLedgerTransaction
                        {
                            Id = id2,
                            Debit = 195,
                            Narrative = "Rates payment",
                            TransactionType = typeof(DebitLedgerTransaction).FullName,
                        },
                    }
                },
                new DataLedgerEntry
                {
                    Balance = 21.15M,
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id3,
                            Credit = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new DataLedgerEntry
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id4,
                            Credit = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new DataLedgerTransaction
                        {
                            Id = id5,
                            Debit = 295.45M,
                            Narrative = "Fix car",
                            TransactionType = typeof(DebitLedgerTransaction).FullName,
                        }
                    }
                }
            });

            var line2 = AddEntryLineForTestData1(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(new[]
            {
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id6,
                            Credit = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id7,
                            Credit = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id8,
                            Credit = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line3 = AddEntryLineForTestData1(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(new[]
            {
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id9,
                            Credit = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id10,
                            Credit = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id11,
                            Credit = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            book.DatedEntries = lines.OrderByDescending(e => e.Date).ToList();
            return book;
        }

        public static DataLedgerBook TestData2()
        {
            var book = new DataLedgerBook
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 2",
                FileName = "C:\\Folder\\FooBook2.xml",
            };

            var lines = new List<DataLedgerEntryLine>();

            var line1 = AddEntryLineForTestData2(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(new[]
            {
                new DataLedgerEntry
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id1,
                            Credit = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new DataLedgerEntry
                {
                    Balance = 21.15M,
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id2,
                            Credit = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new DataLedgerEntry
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id3,
                            Credit = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line2 = AddEntryLineForTestData2(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(new[]
            {
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id4,
                            Credit = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id5,
                            Credit = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id6,
                            Credit = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line3 = AddEntryLineForTestData2(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(new[]
            {
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id7,
                            Credit = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new DataLedgerTransaction
                        {
                            Id = id8,
                            Debit = 195,
                            Narrative = "Rates payment",
                            TransactionType = typeof(DebitLedgerTransaction).FullName,
                        },
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id9,
                            Credit = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new DataLedgerEntry
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<DataLedgerTransaction>
                    {
                        new DataLedgerTransaction
                        {
                            Id = id10,
                            Credit = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new DataLedgerTransaction
                        {
                            Id = id11,
                            Debit = 295.45M,
                            Narrative = "Fix car",
                            TransactionType = typeof(DebitLedgerTransaction).FullName,
                        }
                    }
                }
            });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            book.DatedEntries = lines.OrderByDescending(e => e.Date).ToList();
            return book;
        }

        private static DataLedgerEntryLine AddEntryLineForTestData1(List<DataLedgerEntryLine> entries, DateTime lineDate)
        {
            var line = new DataLedgerEntryLine
            {
                Date = lineDate,
                Remarks = "Lorem ipsum dolor. Mit solo darte.",
            };

            entries.Add(line);
            return line;
        }

        private static DataLedgerEntryLine AddEntryLineForTestData2(List<DataLedgerEntryLine> entries, DateTime lineDate)
        {
            var line = new DataLedgerEntryLine
            {
                Date = lineDate,
                Remarks = "Lorem ipsum dolor. Mit solo darte.",
                BankBalanceAdjustments = new List<DataLedgerTransaction>
                {
                    new DataLedgerTransaction { Debit = 99M, Narrative = "The quick brown fox", TransactionType = typeof(DebitLedgerTransaction).FullName},
                },
            };

            entries.Add(line);
            return line;
        }

        private static void UpdateLineBalances(DataLedgerEntryLine currentLine, DataLedgerEntryLine previousLine, decimal bankBalance)
        {
            currentLine.BankBalance = bankBalance;
            currentLine.BankBalances.Add(new DataBankBalance { Account = StatementModelTestData.ChequeAccount.Name, Balance = bankBalance });

            if (previousLine == null)
            {
                return;
            }

            foreach (var entry in currentLine.Entries)
            {
                var previousEntry = previousLine.Entries.Single(e => e.BucketCode == entry.BucketCode);
                entry.Balance = previousEntry.Balance + entry.Transactions.Sum(t => t.Credit - t.Debit);
                if (entry.Balance < 0)
                {
                    entry.Balance = 0;
                }
            }
        }

    }
}
