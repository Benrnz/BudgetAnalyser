using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.UnitTest.TestData
{
    internal static class LedgerBookDtoTestData
    {
        private static readonly Guid Id1 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A0");
        private static readonly Guid Id10 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A9");
        private static readonly Guid Id11 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6AA");
        private static readonly Guid Id2 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A1");
        private static readonly Guid Id3 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A2");
        private static readonly Guid Id4 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A3");
        private static readonly Guid Id5 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A4");
        private static readonly Guid Id6 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A5");
        private static readonly Guid Id7 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A6");
        private static readonly Guid Id8 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A7");
        private static readonly Guid Id9 = new Guid("2647B4AF-4371-4DA6-B827-E93CCB98B6A8");

        public static LedgerBookDto TestData1()
        {
            var book = new LedgerBookDto
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 1",
                StorageKey = "C:\\Folder\\FooBook.xml"
            };

            var lines = new List<LedgerEntryLineDto>();

            var line1 = AddEntryLineForTestData1(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        Balance = 0, // because would go into negative
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id1,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            },
                            new LedgerTransactionDto
                            {
                                Id = Id2,
                                Amount = -195,
                                Narrative = "Rates payment",
                                TransactionType = typeof(CreditLedgerTransaction).FullName
                            }
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
                                Id = Id3,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id4,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            },
                            new LedgerTransactionDto
                            {
                                Id = Id5,
                                Amount = -295.45M,
                                Narrative = "Fix car",
                                TransactionType = typeof(CreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            var line2 = AddEntryLineForTestData1(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id6,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    },
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RegoBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id7,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id8,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            var line3 = AddEntryLineForTestData1(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id9,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    },
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RegoBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id10,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id11,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            book.Reconciliations = lines.OrderByDescending(e => e.Date).ToList();
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.RegoBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.CarMtcBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.RatesBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            return book;
        }

        public static LedgerBookDto TestData2()
        {
            var book = new LedgerBookDto
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 2",
                StorageKey = "C:\\Folder\\FooBook2.xml"
            };

            var lines = new List<LedgerEntryLineDto>();

            var line1 = AddEntryLineForTestData2(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        Balance = 0, // because would go into negative
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id1,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
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
                                Id = Id2,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id3,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            var line2 = AddEntryLineForTestData2(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id4,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    },
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RegoBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id5,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id6,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            var line3 = AddEntryLineForTestData2(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id7,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            },
                            new LedgerTransactionDto
                            {
                                Id = Id8,
                                Amount = -195,
                                Narrative = "Rates payment",
                                TransactionType = typeof(CreditLedgerTransaction).FullName
                            }
                        }
                    },
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RegoBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id9,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id10,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            },
                            new LedgerTransactionDto
                            {
                                Id = Id11,
                                Amount = -295.45M,
                                Narrative = "Fix car",
                                TransactionType = typeof(CreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            book.Reconciliations = lines.OrderByDescending(e => e.Date).ToList();
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.RegoBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.CarMtcBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.RatesBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            return book;
        }

        /// <summary>
        ///     Same as TestData1 but with some Balance Adjustments on the most recent line.
        ///     The most recent line also has two bank balances associated with it.
        /// </summary>
        public static LedgerBookDto TestData3()
        {
            var book = new LedgerBookDto
            {
                Modified = new DateTime(2013, 12, 14),
                Name = "Test Budget Ledger Book 3",
                StorageKey = "C:\\Folder\\FooBook3.xml"
            };

            var lines = new List<LedgerEntryLineDto>();

            var line1 = AddEntryLineForTestData1(lines, new DateTime(2013, 12, 20));
            line1.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        Balance = 0, // because would go into negative
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id1,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            },
                            new LedgerTransactionDto
                            {
                                Id = Id2,
                                Amount = -195,
                                Narrative = "Rates payment",
                                TransactionType = typeof(CreditLedgerTransaction).FullName
                            }
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
                                Id = Id3,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id4,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            },
                            new LedgerTransactionDto
                            {
                                Id = Id5,
                                Amount = -295.45M,
                                Narrative = "Fix car",
                                TransactionType = typeof(CreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            var line2 = AddEntryLineForTestData1(lines, new DateTime(2014, 1, 20));
            line2.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id6,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    },
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RegoBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id7,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id8,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            var line3 = AddEntryLineForTestData1(lines, new DateTime(2014, 02, 20));
            line3.Entries.AddRange(
                new[]
                {
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RatesBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id9,
                                Amount = 75,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    },
                    new LedgerEntryDto
                    {
                        BucketCode = TestDataConstants.RegoBucketCode,
                        Transactions = new List<LedgerTransactionDto>
                        {
                            new LedgerTransactionDto
                            {
                                Id = Id10,
                                Amount = 21.15M,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
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
                                Id = Id11,
                                Amount = 95,
                                Narrative = "Budgeted Amount",
                                TransactionType = typeof(BudgetCreditLedgerTransaction).FullName
                            }
                        }
                    }
                });

            line3.BankBalanceAdjustments.Add(
                new LedgerTransactionDto
                {
                    Account = StatementModelTestData.ChequeAccount.Name,
                    Amount = -100.01M,
                    Narrative = "Visa payment yet to go out",
                    Id = new Guid("22927CF0-BAA2-4828-A669-C77396888BD6"),
                    TransactionType = typeof(CreditLedgerTransaction).FullName
                });

            UpdateLineBalances(line1, null, 1999.25M);
            UpdateLineBalances(line2, line1, 2001.15M);
            UpdateLineBalances(line3, line2, 1801.45M);
            line3.BankBalances.Add(
                new BankBalanceDto
                {
                    Account = StatementModelTestData.SavingsAccount.Name,
                    Balance = 201M
                });
            line3.BankBalance += 201M;

            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.RegoBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.CarMtcBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Ledgers.Add(new LedgerBucketDto { BucketCode = TestDataConstants.RatesBucketCode, StoredInAccount = TestDataConstants.ChequeAccountName });
            book.Reconciliations = lines.OrderByDescending(e => e.Date).ToList();
            return book;
        }

        private static LedgerEntryLineDto AddEntryLineForTestData1(List<LedgerEntryLineDto> entries, DateTime lineDate)
        {
            var line = new LedgerEntryLineDto
            {
                Date = lineDate,
                Remarks = "Lorem ipsum dolor. Mit solo darte."
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
                    new LedgerTransactionDto { Amount = -99M, Narrative = "The quick brown fox", TransactionType = typeof(CreditLedgerTransaction).FullName }
                }
            };

            entries.Add(line);
            return line;
        }

        private static void UpdateLineBalances(LedgerEntryLineDto currentLine, LedgerEntryLineDto previousLine, decimal bankBalance)
        {
            currentLine.BankBalance = bankBalance;
            currentLine.BankBalances.Add(new BankBalanceDto { Account = StatementModelTestData.ChequeAccount.Name, Balance = bankBalance });

            if (previousLine is null)
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
