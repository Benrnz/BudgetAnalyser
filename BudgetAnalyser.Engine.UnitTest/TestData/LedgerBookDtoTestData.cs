using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.Engine.UnitTest.TestData;

internal static class LedgerBookDtoTestData
{
    private static readonly Guid Id1 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A0");
    private static readonly Guid Id10 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A9");
    private static readonly Guid Id11 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6AA");
    private static readonly Guid Id2 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A1");
    private static readonly Guid Id3 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A2");
    private static readonly Guid Id4 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A3");
    private static readonly Guid Id5 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A4");
    private static readonly Guid Id6 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A5");
    private static readonly Guid Id7 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A6");
    private static readonly Guid Id8 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A7");
    private static readonly Guid Id9 = new("2647B4AF-4371-4DA6-B827-E93CCB98B6A8");


    /// <summary>
    ///     A LedgerBook with Rates, Rego, CarMtc, and a few transactions each.
    ///     It has three EntryLines dated 2013-12-20, 2014-01-20, 2014-02-20.
    /// </summary>
    public static LedgerBookDto TestData1()
    {
        var ledgerBookDto = new LedgerBookDto
        (
            Modified: new DateTime(2013, 12, 14, 0, 0, 0, DateTimeKind.Utc),
            Name: "Test Budget Ledger Book 1",
            StorageKey: "C:\\Folder\\FooBook.xml",
            Reconciliations:
            [
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2014, 02, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            150M,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto
                                (
                                    Id: Id9, Amount: 75, Narrative: "Budgeted Amount",
                                    TransactionType: "BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction", AutoMatchingReference: null, Account: TestDataConstants.ChequeAccountName, Date:
                                    new DateOnly(2014, 2, 20)
                                )
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            63.45M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto
                                (
                                    Id: Id10, Amount: 21.15M, Narrative: "Budgeted Amount",
                                    TransactionType: "BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction", AutoMatchingReference: null, Account: TestDataConstants.ChequeAccountName, Date:
                                    new DateOnly(2014, 2, 20)
                                )
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            190M,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto
                                (
                                    Id: Id11, Amount: 95, Narrative: "Budgeted Amount",
                                    TransactionType: "BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction", AutoMatchingReference: null, Account: TestDataConstants.ChequeAccountName, Date:
                                    new DateOnly(2014, 2, 20)
                                )
                            ]
                        )
                    ],
                    BankBalance: 1801.45M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 1801.45M)],
                    BankBalanceAdjustments: []
                ),
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2014, 01, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            75M,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id6, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName, Account:
                                    TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            42.30M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id7, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName, Account:
                                    TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            95M,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id8, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName, AutoMatchingReference:
                                    null, Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20))
                            ]
                        )
                    ],
                    BankBalance: 2001.15M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 2001.15M)],
                    BankBalanceAdjustments: []
                ),
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2013, 12, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            0,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id1, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName, Account:
                                    TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null),
                                new LedgerTransactionDto(Id: Id2, Amount: -195, Narrative: "Rates payment", TransactionType: typeof(CreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            21.15M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id3, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            0,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id4, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName, Account:
                                    TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null),
                                new LedgerTransactionDto(Id: Id5, Amount: -295.45M, Narrative: "Fix car", TransactionType: typeof(CreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, AutoMatchingReference: null, Date: new DateOnly(2013, 12, 20))
                            ]
                        )
                    ],
                    BankBalance: 1999.25M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 1999.25M)],
                    BankBalanceAdjustments: []
                )
            ],
            Ledgers:
            [
                new LedgerBucketDto(TestDataConstants.RegoBucketCode, TestDataConstants.ChequeAccountName),
                new LedgerBucketDto(TestDataConstants.CarMtcBucketCode, TestDataConstants.ChequeAccountName),
                new LedgerBucketDto(TestDataConstants.RatesBucketCode, TestDataConstants.ChequeAccountName)
            ],
            Checksum: 0,
            MobileSettings: null
        );
        return ledgerBookDto;
    }

    /// <summary>
    ///     Copy of TestData1. It still has three EntryLines  dated 2013-12-20, 2014-01-20, 2014-02-20.
    ///     Each reconciliation line has a bank balance adjustment of -$99
    /// </summary>
    public static LedgerBookDto TestData2()
    {
        var ledgerBookDto = new LedgerBookDto
        (
            Modified: new DateTime(2013, 12, 14, 0, 0, 0, DateTimeKind.Utc),
            Name: "Test Budget Ledger Book 2",
            StorageKey: "C:\\Folder\\FooBook2.xml",
            Reconciliations:
            [
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2014, 02, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            0M,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id7, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 2, 20), AutoMatchingReference: null),
                                new LedgerTransactionDto(Id: Id8, Amount: -195, Narrative: "Rates payment", TransactionType: typeof(CreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, AutoMatchingReference: null, Date: new DateOnly(2014, 2, 20))
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            63.45M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto
                                (
                                    Id: Id9, Amount: 21.15M, Narrative: "Budgeted Amount",
                                    TransactionType: "BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction", AutoMatchingReference: null, Account: TestDataConstants.ChequeAccountName,
                                    Date: new DateOnly(2014, 2, 20)
                                )
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            0M,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id10, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, AutoMatchingReference: null, Date: new DateOnly(2014, 2, 20)),
                                new LedgerTransactionDto(Id: Id11, Amount: -295.45M, Narrative: "Fix car", TransactionType: typeof(CreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, AutoMatchingReference: null, Date: new DateOnly(2014, 2, 20))
                            ]
                        )
                    ],
                    BankBalance: 1801.45M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 1801.45M)],
                    BankBalanceAdjustments:
                    [
                        new LedgerTransactionDto
                        (
                            Amount: -99,
                            Narrative: "The quick brown fox",
                            TransactionType: typeof(BankBalanceAdjustmentTransaction).FullName,
                            Account: TestDataConstants.ChequeAccountName,
                            AutoMatchingReference: null,
                            Date: new DateOnly(2014, 2, 20),
                            Id: new Guid("22927CF0-BAA2-4828-A669-C77396888BD6")
                        )
                    ]
                ),
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2014, 01, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            75M,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id4, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            42.30M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id5, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            95M,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id6, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        )
                    ],
                    BankBalance: 2001.15M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 2001.15M)],
                    BankBalanceAdjustments:
                    [
                        new LedgerTransactionDto
                        (
                            Amount: -99,
                            Narrative: "The quick brown fox",
                            TransactionType: typeof(BankBalanceAdjustmentTransaction).FullName,
                            Account: TestDataConstants.ChequeAccountName,
                            AutoMatchingReference: null,
                            Date: new DateOnly(2014, 1, 20),
                            Id: new Guid("22927CF0-BAA2-4828-A669-C77396888BD6")
                        )
                    ]
                ),
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2013, 12, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            0,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id1, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            21.15M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id2, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            0,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id3, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        )
                    ],
                    BankBalance: 1999.25M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 1999.25M)],
                    BankBalanceAdjustments:
                    [
                        new LedgerTransactionDto
                        (
                            Amount: -99,
                            Narrative: "The quick brown fox",
                            TransactionType: typeof(BankBalanceAdjustmentTransaction).FullName,
                            Account: TestDataConstants.ChequeAccountName,
                            Date: new DateOnly(2013, 12, 20),
                            AutoMatchingReference: null,
                            Id: new Guid("22927CF0-BAA2-4828-A669-C77396888B99")
                        )
                    ]
                )
            ],
            Ledgers:
            [
                new LedgerBucketDto(TestDataConstants.RegoBucketCode, TestDataConstants.ChequeAccountName),
                new LedgerBucketDto(TestDataConstants.CarMtcBucketCode, TestDataConstants.ChequeAccountName),
                new LedgerBucketDto(TestDataConstants.RatesBucketCode, TestDataConstants.ChequeAccountName)
            ],
            Checksum: 0,
            MobileSettings: null
        );
        return ledgerBookDto;
    }


    /// <summary>
    ///     Same as TestData1 but with some Balance Adjustments on the most recent line.
    ///     The most recent line also has two bank balances associated with it.
    /// </summary>
    public static LedgerBookDto TestData3()
    {
        var ledgerBookDto = new LedgerBookDto
        (
            Modified: new DateTime(2013, 12, 14, 0, 0, 0, DateTimeKind.Utc),
            Name: "Test Budget Ledger Book 3",
            StorageKey: "C:\\Folder\\FooBook3.xml",
            Reconciliations:
            [
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2014, 02, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            150M,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id9, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 2, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            63.45M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto
                                (Id: Id10, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: "BudgetAnalyser.Engine.Ledger.BudgetCreditLedgerTransaction", AutoMatchingReference: null,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 2, 20))
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            190M,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id11, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 2, 20), AutoMatchingReference: null)
                            ]
                        )
                    ],
                    BankBalance: 2002.45M,
                    BankBalances:
                    [
                        new BankBalanceDto(TestDataConstants.ChequeAccountName, 1801.45M),
                        new BankBalanceDto(TestDataConstants.SavingsAccountName, 201M)
                    ],
                    BankBalanceAdjustments:
                    [
                        new LedgerTransactionDto
                        (
                            StatementModelTestData.ChequeAccount.Name,
                            -100.01M,
                            Narrative: "Visa payment yet to go out",
                            Id: new Guid("22927CF0-BAA2-4828-A669-C77396888BD6"),
                            TransactionType: typeof(CreditLedgerTransaction).FullName,
                            AutoMatchingReference: null,
                            Date: new DateOnly(2014, 2, 20)
                        )
                    ]
                ),
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2014, 01, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            75M,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id6, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            42.30M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id7, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            95M,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id8, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2014, 1, 20), AutoMatchingReference: null)
                            ]
                        )
                    ],
                    BankBalance: 2001.15M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 2001.15M)],
                    BankBalanceAdjustments: []
                ),
                new LedgerEntryLineDto
                (
                    Date: new DateOnly(2013, 12, 20),
                    Remarks: "Lorem ipsum dolor. Mit solo darte.",
                    Entries:
                    [
                        new LedgerEntryDto
                        (
                            0,
                            TestDataConstants.RatesBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id1, Amount: 75, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null),
                                new LedgerTransactionDto(Id: Id2, Amount: -195, Narrative: "Rates payment", TransactionType: typeof(CreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            21.15M,
                            TestDataConstants.RegoBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id3, Amount: 21.15M, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        ),

                        new LedgerEntryDto
                        (
                            0,
                            TestDataConstants.CarMtcBucketCode,
                            TestDataConstants.ChequeAccountName,
                            [
                                new LedgerTransactionDto(Id: Id4, Amount: 95, Narrative: "Budgeted Amount", TransactionType: typeof(BudgetCreditLedgerTransaction).FullName,
                                    Account: TestDataConstants.ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null),
                                new LedgerTransactionDto(Id: Id5, Amount: -295.45M, Narrative: "Fix car", TransactionType: typeof(CreditLedgerTransaction).FullName, Account: TestDataConstants
                                    .ChequeAccountName, Date: new DateOnly(2013, 12, 20), AutoMatchingReference: null)
                            ]
                        )
                    ],
                    BankBalance: 1999.25M,
                    BankBalances: [new BankBalanceDto(TestDataConstants.ChequeAccountName, 1999.25M)],
                    BankBalanceAdjustments: []
                )
            ],
            Ledgers:
            [
                new LedgerBucketDto(TestDataConstants.RegoBucketCode, TestDataConstants.ChequeAccountName),
                new LedgerBucketDto(TestDataConstants.CarMtcBucketCode, TestDataConstants.ChequeAccountName),
                new LedgerBucketDto(TestDataConstants.RatesBucketCode, TestDataConstants.ChequeAccountName)
            ],
            Checksum: 0,
            MobileSettings: null
        );
        return ledgerBookDto;
    }
}
