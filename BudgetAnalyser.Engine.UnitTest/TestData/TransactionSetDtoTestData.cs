using BudgetAnalyser.Engine.Statement.Data;

namespace BudgetAnalyser.Engine.UnitTest.TestData;

public static class TransactionSetDtoTestData
{
    public static TransactionSetDto TestData2()
    {
        return new TransactionSetDto
        (
            Checksum: 252523523525,
            StorageKey: @"C:\Foo\Bar.csv",
            LastImport: new DateTime(2013, 8, 15),
            VersionHash: "uiwhgr8972y59872gh5972798gh",
            Transactions:
            [
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    -95.15M,
                    BudgetBucketCode: TestDataConstants.PowerBucketCode,
                    Date: new DateOnly(2013, 7, 15),
                    Description: "Engery Online Electricity",
                    Id: new Guid("b227e353-cfe4-4e5d-a045-2f31cdea3412"),
                    Reference1: "12334458989",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    -58.19M,
                    BudgetBucketCode: TestDataConstants.PhoneBucketCode,
                    Date: new DateOnly(2013, 7, 16),
                    Description: "Vodafone Mobile Ltd",
                    Id: new Guid("e669028f-0214-4d14-8b15-14639e25c9a0"),
                    Reference1: "1233411119",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    850.99M,
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Date: new DateOnly(2013, 7, 20),
                    Description: "Payroll",
                    Id: new Guid("8d85c973-77f3-48cc-aec8-edb86b06faf5"),
                    Reference1: "123xxxxxx89",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    -89.15M,
                    BudgetBucketCode: TestDataConstants.PowerBucketCode,
                    Date: new DateOnly(2013, 8, 15),
                    Description: "Engery Online Electricity",
                    Id: new Guid("8abda989-5127-476b-b96a-98660d7d84f1"),
                    Reference1: "12334458989",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.VisaAccountName,
                    -91.00M,
                    BudgetBucketCode: TestDataConstants.CarMtcBucketCode,
                    Date: new DateOnly(2013, 8, 15),
                    Description: "Ford Ellerslie",
                    Id: new Guid("54bc8f8e-52c2-49e7-b12f-0a662bb23ec2"),
                    Reference1: "23411222",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    -68.29M,
                    BudgetBucketCode: TestDataConstants.PhoneBucketCode,
                    Date: new DateOnly(2013, 8, 16),
                    Description: "Vodafone Mobile Ltd",
                    Id: new Guid("932b224f-66f3-4ba2-a015-b5f7c03baa8f"),
                    Reference1: "1233411119",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    850.99M,
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Date: new DateOnly(2013, 8, 20),
                    Description: "Payroll",
                    Id: new Guid("13ba59b9-052c-4eed-bf37-65b3d3af0aea"),
                    Reference1: "123xxxxxx89",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.VisaAccountName,
                    -55.00M,
                    BudgetBucketCode: TestDataConstants.HairBucketCode,
                    Date: new DateOnly(2013, 8, 22),
                    Description: "Rodney Wayne",
                    Id: new Guid("058a140b-b6bc-4933-b017-7970a773a69c"),
                    Reference1: "1233411222",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.VisaAccountName,
                    -350.00M,
                    BudgetBucketCode: TestDataConstants.RegoBucketCode,
                    Date: new DateOnly(2013, 9, 1),
                    Description: "nzpost nzta car regisration",
                    Id: new Guid("300c7e27-b057-4cd3-8e74-d7058290d4f8"),
                    Reference1: "23411222",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                ),
                new TransactionDto(
                    TestDataConstants.ChequeAccountName,
                    850.99M,
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Date: new DateOnly(2013, 9, 20),
                    Description: "Payroll",
                    Id: new Guid("1465431b-5991-481a-94da-704ecbfe6c20"),
                    Reference1: "123xxxxxx89",
                    Reference2: "",
                    Reference3: "",
                    TransactionType: "Bill Payment"
                )
            ]
        );
    }
}
