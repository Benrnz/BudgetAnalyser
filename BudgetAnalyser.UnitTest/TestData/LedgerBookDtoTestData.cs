﻿using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.UnitTest.TestData
{
    internal static class LedgerBookDtoTestData
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

        public static LedgerBookDto TestData1()
        {
            var book = new LedgerBookDto
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 1",
                FileName = "C:\\Folder\\FooBook.xml",
            };

            var lines = new List<LedgerEntryLineDto>();

            var line1 = AddEntryLineForTestData1(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id1,
                            Amount = 75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new LedgerTransactionDto
                        {
                            Id = id2,
                            Amount = -195,
                            Narrative = "Rates payment",
                            TransactionType = typeof(CreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    Balance = 21.15M,
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id3,
                            Amount = 21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id4,
                            Amount = 95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new LedgerTransactionDto
                        {
                            Id = id5,
                            Amount = -295.45M,
                            Narrative = "Fix car",
                            TransactionType = typeof(CreditLedgerTransaction).FullName,
                        }
                    }
                }
            });

            var line2 = AddEntryLineForTestData1(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id6,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id7,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id8,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line3 = AddEntryLineForTestData1(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id9,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id10,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id11,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            book.Reconciliations = lines.OrderByDescending(e => e.Date).ToList();
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.RegoBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.CarMtcBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.RatesBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            return book;
        }

        public static LedgerBookDto TestData2()
        {
            var book = new LedgerBookDto
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 2",
                FileName = "C:\\Folder\\FooBook2.xml",
            };

            var lines = new List<LedgerEntryLineDto>();

            var line1 = AddEntryLineForTestData2(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id1,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    Balance = 21.15M,
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id2,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id3,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line2 = AddEntryLineForTestData2(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id4,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id5,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id6,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line3 = AddEntryLineForTestData2(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id7,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new LedgerTransactionDto
                        {
                            Id = id8,
                            Amount = -195,
                            Narrative = "Rates payment",
                            TransactionType = typeof(CreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id9,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id10,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new LedgerTransactionDto
                        {
                            Id = id11,
                            Amount = -295.45M,
                            Narrative = "Fix car",
                            TransactionType = typeof(CreditLedgerTransaction).FullName,
                        }
                    }
                }
            });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            book.Reconciliations = lines.OrderByDescending(e => e.Date).ToList();
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.RegoBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.CarMtcBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.RatesBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            return book;
        }

        /// <summary>
        /// Same as TestData1 but with some Balance Adjustments on the most recent line.
        /// The most recent line also has two bank balances associated with it.
        /// </summary>
        public static LedgerBookDto TestData3()
        {
            var book = new LedgerBookDto
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 3",
                FileName = "C:\\Folder\\FooBook3.xml",
            };

            var lines = new List<LedgerEntryLineDto>();

            var line1 = AddEntryLineForTestData1(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id1,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new LedgerTransactionDto
                        {
                            Id = id2,
                            Amount = -195,
                            Narrative = "Rates payment",
                            TransactionType = typeof(CreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    Balance = 21.15M,
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id3,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    Balance = 0, // because would go into negative
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id4,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                        new LedgerTransactionDto
                        {
                            Id = id5,
                            Amount = -295.45M,
                            Narrative = "Fix car",
                            TransactionType = typeof(CreditLedgerTransaction).FullName,
                        }
                    }
                }
            });

            var line2 = AddEntryLineForTestData1(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id6,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id7,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id8,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            var line3 = AddEntryLineForTestData1(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(new[]
            {
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id9,
                            Amount =75,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id10,
                            Amount =21.15M,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        }
                    }
                },
                new LedgerEntryDto
                {
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Transactions = new List<LedgerTransactionDto>
                    {
                        new LedgerTransactionDto
                        {
                            Id = id11,
                            Amount =95,
                            Narrative = "Budgeted Amount",
                            TransactionType = typeof(BudgetCreditLedgerTransaction).FullName,
                        },
                    }
                }
            });

            line3.BankBalanceAdjustments.Add(new LedgerTransactionDto
            {
                AccountType = StatementModelTestData.ChequeAccount.Name,
                Amount = -100.01M,
                Narrative = "Visa payment yet to go out",
                Id = new Guid("22927CF0-BAA2-4828-A669-C77396888BD6"),
                TransactionType = typeof(CreditLedgerTransaction).FullName,
            });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            line3.BankBalances.Add(
                new BankBalanceDto
                {
                    Account = StatementModelTestData.SavingsAccount.Name,
                    Balance = 201M,
                });
            line3.BankBalance += 201M;

            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.RegoBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.CarMtcBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerColumnDto { BucketCode = TestDataConstants.RatesBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Reconciliations = lines.OrderByDescending(e => e.Date).ToList();
            return book;
        }

        private static LedgerEntryLineDto AddEntryLineForTestData1(List<LedgerEntryLineDto> entries, DateTime lineDate)
        {
            var line = new LedgerEntryLineDto
            {
                Date = lineDate,
                Remarks = "Lorem ipsum dolor. Mit solo darte.",
            };

            entries.Add(line);
            return line;
        }

        private static LedgerEntryLineDto AddEntryLineForTestData2(List<LedgerEntryLineDto> entries, DateTime lineDate)
        {
            var line = new LedgerEntryLineDto
            {
                Date = lineDate,
                Remarks = "Lorem ipsum dolor. Mit solo darte.",
                BankBalanceAdjustments = new List<LedgerTransactionDto>
                {
                    new LedgerTransactionDto { Amount = -99M, Narrative = "The quick brown fox", TransactionType = typeof(CreditLedgerTransaction).FullName},
                },
            };

            entries.Add(line);
            return line;
        }

        private static void UpdateLineBalances(LedgerEntryLineDto currentLine, LedgerEntryLineDto previousLine, decimal bankBalance)
        {
            currentLine.BankBalance = bankBalance;
            currentLine.BankBalances.Add(new BankBalanceDto { Account = StatementModelTestData.ChequeAccount.Name, Balance = bankBalance });

            if (previousLine == null)
            {
                return;
            }

            foreach (var entry in currentLine.Entries)
            {
                var previousEntry = previousLine.Entries.Single(e => e.BucketCode == entry.BucketCode);
                entry.Balance = previousEntry.Balance + entry.Transactions.Sum(t => t.Amount);
                if (entry.Balance < 0)
                {
                    entry.Balance = 0;
                }
            }
        }

    }
}
