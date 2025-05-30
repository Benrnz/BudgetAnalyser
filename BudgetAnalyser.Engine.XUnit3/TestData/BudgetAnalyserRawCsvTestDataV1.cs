﻿using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement.Data;

namespace BudgetAnalyser.Engine.XUnit.TestData;

public static class BudgetAnalyserRawCsvTestDataV1
{
    public static TransactionSetDto BadTestData_CorruptedCommaFormat()
    {
        var set = new TransactionSetDto
        (
            StorageKey: "abcdef",
            LastImport: new DateTime(2013, 8, 15),
            Transactions:
            [
                new TransactionDto(
                    TransactionType: "Direct Credit,Yee,Har,Boi,Crazy-commas,",
                    Description: "Smith A B & J S",
                    Amount: 3474.02M,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.PayCreditCardBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("7f78bd65-017e-4337-9459-7e5dfa447d66")
                ),
                new TransactionDto(
                    TransactionType: "Salary",
                    Description: "Ipayroll Limite",
                    Reference1: "Acme Inc",
                    Reference2: "",
                    Reference3: "Ipayroll",
                    Amount: 3000M,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("4e6bad3b-89c7-458c-a993-38859da68b54")
                ),
                new TransactionDto(
                    TransactionType: "Direct Credit",
                    Description: "Acme Inc Ltd      Dl",
                    Reference1: "Salary",
                    Reference2: "Acme Inc",
                    Reference3: "Acme Inc",
                    Amount: 144M,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("55f94f77-601b-4808-ab38-b71cd5f4d6f7")
                ),
                new TransactionDto(
                    TransactionType: "Direct Credit",
                    Description: "Acme Inc Ltd      Dl",
                    Reference1: "Salary",
                    Reference2: "Acme Inc",
                    Reference3: "Acme Inc",
                    Amount: 3000,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("1c4d9d4c-46de-4c39-a2f4-293beb646cc8")
                ),
                new TransactionDto(
                    TransactionType: "Chq/Withdrawal",
                    Description: "",
                    Reference1: "0894T0129180",
                    Reference2: "",
                    Reference3: "",
                    Amount: -550M,
                    Date: new DateOnly(2012, 08, 16),
                    BudgetBucketCode: SurplusBucket.SurplusCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("780f11dd-5ce4-4562-837c-f6043bdca27f")
                ),
                new TransactionDto(
                    TransactionType: "Credit Card Debit",
                    Description: "Charcoal Burger          Thames   Nz",
                    Reference1: "0894T0129180",
                    Reference2: "4367-****-****-3239",
                    Reference3: "",
                    Amount: -27.50M,
                    Date: new DateOnly(2012, 08, 17),
                    BudgetBucketCode: SurplusBucket.SurplusCode,
                    Account: TestDataConstants.VisaAccountName,
                    Id: new Guid("136e9010-9d07-4ddb-b7a3-abd209a23f44")
                ),
                new TransactionDto(
                    TransactionType: "Credit Card Debit",
                    Description: "Charcoal Burger          Thames   Nz",
                    Reference1: "0894T0129180",
                    Reference2: "4367-****-****-3239",
                    Reference3: "",
                    Amount: -13.50M,
                    Date: new DateOnly(2012, 08, 17),
                    BudgetBucketCode: SurplusBucket.SurplusCode,
                    Account: TestDataConstants.VisaAccountName,
                    Id: new Guid("136e9010-9d07-4ddb-b7a3-abd209a23fff")
                )
            ]
        );

        return set;
    }

    public static string BadTestData_IncorrectChecksum()
    {
        return """"
               VersionHash,15955E20-A2CC-4C69-AD42-94D84377FC0C,TransactionCheckSum,5287889576422253805,2012-08-20T00:00:00.0000000Z
               Direct Credit,Smith A B & J S,,,,3474.02,2012-08-15T00:00:00.0000000,PAYCC,CHEQUE,7f78bd65-017e-4337-9459-7e5dfa447d66,
               Salary,Ipayroll Limite,Acme Inc,,Ipayroll,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,4e6bad3b-89c7-458c-a993-38859da68b54,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,55f94f77-601b-4808-ab38-b71cd5f4d6f7,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,144.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,1c4d9d4c-46de-4c39-a2f4-293beb646cc8,
               Chq/Withdrawal,,0894T0129180,,,-550.00,2012-08-16T00:00:00.0000000,SURPLUS,CHEQUE,780f11dd-5ce4-4562-837c-f6043bdca27f,
               Credit Card Debit,Charcoal Burger          Thames   Nz,4367-****-****-3239,,,-27.50,2012-08-17T00:00:00.0000000,SURPLUS,VISA,136e9010-9d07-4ddb-b7a3-abd209a23f44,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-14.00,2012-08-17T00:00:00.0000000,SURPLUS,VISA,17ef7740-7714-467a-be8e-32089268d1d0,
               Credit Card Debit,Mcdonalds Queen        Auckland     Nz,4367-****-****-3239,,,-8.50,2012-08-18T00:00:00.0000000,SURPLUS,VISA,1d207cd2-48ca-44b7-b0ea-364ed73ffb75,
               Credit Card Debit,Paknsave Thames      Thames   Nz,4367-****-****-3239,,,-224.80,2012-08-19T00:00:00.0000000,FOOD,VISA,5dddf943-5c93-4e8f-b928-ecd0ceb14af5,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-23.50,2012-08-19T00:00:00.0000000,SURPLUS,VISA,2af5eaf4-9bed-49b0-8463-0fbca12d7cdc,
               Credit Card Debit,Placemakers Mt Wgtn      Auckland     Nz,4367-****-****-3239,.,,-76.96,2012-08-19T00:00:00.0000000,SURPLUS,VISA,58dd11a6-2ff1-4e85-9948-56090d47cb27,
               Credit Card Debit,Paknsave Fuel Thames Thames   Nz,4367-****-****-3239,,,-78.63,2012-08-19T00:00:00.0000000,FUEL,VISA,c8049b2e-2662-48aa-b407-762269ff42bf,
               Payment,The Very Big Telco,202610963,095501328,B Smith,-91.98,2012-08-20T00:00:00.0000000,PHNET,CHEQUE,56b22788-d1e6-4a5d-a715-08b563dae678,
               Payment,The very cool Power Co,A B Smith,659792,Energyonline,-212.07,2012-08-20T00:00:00.0000000,POWER,CHEQUE,17b8c91c-5174-41a7-b382-b6a9ff52a11a,
               Credit Card Debit,Z Queen Street            Auckland     Nz,4367-****-****-3239,,,-28.49,2012-08-20T00:00:00.0000000,FUEL,VISA,bd12376a-6aac-4171-a23c-f85af168582f,
               """";
    }

    public static string BadTestData_IncorrectDataTypeInRow1()
    {
        return """"
               VersionHash,15955E20-A2CC-4C69-AD42-94D84377FC0C,TransactionCheckSum,-3141538081309485159,2012-08-20T00:00:00.0000000Z
               Direct Credit,Smith A B & J S,,,,ABCDEFG,2012-08-15T00:00:00.0000000,PAYCC,CHEQUE,7f78bd65-017e-4337-9459-7e5dfa447d66,
               Salary,Ipayroll Limite,Acme Inc,,Ipayroll,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,4e6bad3b-89c7-458c-a993-38859da68b54,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,55f94f77-601b-4808-ab38-b71cd5f4d6f7,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,144.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,1c4d9d4c-46de-4c39-a2f4-293beb646cc8,
               Chq/Withdrawal,,0894T0129180,,,-550.00,2012-08-16T00:00:00.0000000,SURPLUS,CHEQUE,780f11dd-5ce4-4562-837c-f6043bdca27f,
               Credit Card Debit,Charcoal Burger          Thames   Nz,4367-****-****-3239,,,-27.50,2012-08-17T00:00:00.0000000,SURPLUS,VISA,136e9010-9d07-4ddb-b7a3-abd209a23f44,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-14.00,2012-08-17T00:00:00.0000000,SURPLUS,VISA,17ef7740-7714-467a-be8e-32089268d1d0,
               Credit Card Debit,Mcdonalds Queen        Auckland     Nz,4367-****-****-3239,,,-8.50,2012-08-18T00:00:00.0000000,SURPLUS,VISA,1d207cd2-48ca-44b7-b0ea-364ed73ffb75,
               Credit Card Debit,Paknsave Thames      Thames   Nz,4367-****-****-3239,,,-224.80,2012-08-19T00:00:00.0000000,FOOD,VISA,5dddf943-5c93-4e8f-b928-ecd0ceb14af5,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-23.50,2012-08-19T00:00:00.0000000,SURPLUS,VISA,2af5eaf4-9bed-49b0-8463-0fbca12d7cdc,
               Credit Card Debit,Placemakers Mt Wgtn      Auckland     Nz,4367-****-****-3239,.,,-76.96,2012-08-19T00:00:00.0000000,SURPLUS,VISA,58dd11a6-2ff1-4e85-9948-56090d47cb27,
               Credit Card Debit,Paknsave Fuel Thames Thames   Nz,4367-****-****-3239,,,-78.63,2012-08-19T00:00:00.0000000,FUEL,VISA,c8049b2e-2662-48aa-b407-762269ff42bf,
               Payment,The Very Big Telco,202610963,095501328,B Smith,-91.98,2012-08-20T00:00:00.0000000,PHNET,CHEQUE,56b22788-d1e6-4a5d-a715-08b563dae678,
               Payment,The very cool Power Co,A B Smith,659792,Energyonline,-212.07,2012-08-20T00:00:00.0000000,POWER,CHEQUE,17b8c91c-5174-41a7-b382-b6a9ff52a11a,
               Credit Card Debit,Z Queen Street            Auckland     Nz,4367-****-****-3239,,,-28.49,2012-08-20T00:00:00.0000000,FUEL,VISA,bd12376a-6aac-4171-a23c-f85af168582f,
               """";
    }

    public static string BadTestData_IncorrectVersionHash()
    {
        return """"
               VersionHash,11111111-2222-3333-4444-55555555,TransactionCheckSum,6704009528804600641,2012-08-20T00:00:00.0000000Z
               Direct Credit,Smith A B & J S,,,,3474.02,2012-08-15T00:00:00.0000000,PAYCC,CHEQUE,7f78bd65-017e-4337-9459-7e5dfa447d66,
               Salary,Ipayroll Limite,Acme Inc,,Ipayroll,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,4e6bad3b-89c7-458c-a993-38859da68b54,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,55f94f77-601b-4808-ab38-b71cd5f4d6f7,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,144.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,1c4d9d4c-46de-4c39-a2f4-293beb646cc8,
               Chq/Withdrawal,,0894T0129180,,,-550.00,2012-08-16T00:00:00.0000000,SURPLUS,CHEQUE,780f11dd-5ce4-4562-837c-f6043bdca27f,
               Credit Card Debit,Charcoal Burger          Thames   Nz,4367-****-****-3239,,,-27.50,2012-08-17T00:00:00.0000000,SURPLUS,VISA,136e9010-9d07-4ddb-b7a3-abd209a23f44,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-14.00,2012-08-17T00:00:00.0000000,SURPLUS,VISA,17ef7740-7714-467a-be8e-32089268d1d0,
               Credit Card Debit,Mcdonalds Queen        Auckland     Nz,4367-****-****-3239,,,-8.50,2012-08-18T00:00:00.0000000,SURPLUS,VISA,1d207cd2-48ca-44b7-b0ea-364ed73ffb75,
               Credit Card Debit,Paknsave Thames      Thames   Nz,4367-****-****-3239,,,-224.80,2012-08-19T00:00:00.0000000,FOOD,VISA,5dddf943-5c93-4e8f-b928-ecd0ceb14af5,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-23.50,2012-08-19T00:00:00.0000000,SURPLUS,VISA,2af5eaf4-9bed-49b0-8463-0fbca12d7cdc,
               Credit Card Debit,Placemakers Mt Wgtn      Auckland     Nz,4367-****-****-3239,.,,-76.96,2012-08-19T00:00:00.0000000,SURPLUS,VISA,58dd11a6-2ff1-4e85-9948-56090d47cb27,
               Credit Card Debit,Paknsave Fuel Thames Thames   Nz,4367-****-****-3239,,,-78.63,2012-08-19T00:00:00.0000000,FUEL,VISA,c8049b2e-2662-48aa-b407-762269ff42bf,
               Payment,The Very Big Telco,202610963,095501328,B Smith,-91.98,2012-08-20T00:00:00.0000000,PHNET,CHEQUE,56b22788-d1e6-4a5d-a715-08b563dae678,
               Payment,The very cool Power Co,A B Smith,659792,Energyonline,-212.07,2012-08-20T00:00:00.0000000,POWER,CHEQUE,17b8c91c-5174-41a7-b382-b6a9ff52a11a,
               Credit Card Debit,Z Queen Street            Auckland     Nz,4367-****-****-3239,,,-28.49,2012-08-20T00:00:00.0000000,FUEL,VISA,bd12376a-6aac-4171-a23c-f85af168582f,

               """";
    }

    public static string EmptyTestData()
    {
        return """"
               VersionHash,15955E20-A2CC-4C69-AD42-94D84377FC0C,TransactionCheckSum,0,2012-08-20T00:00:00.0000000Z

               """";
    }

    public static string TestData1()
    {
        return """"
               VersionHash,15955E20-A2CC-4C69-AD42-94D84377FC0C,TransactionCheckSum,6704009528804600641,2012-08-20T00:00:00.0000000Z
               Direct Credit,Smith A B & J S,,,,3474.02,2012-08-15T00:00:00.0000000,PAYCC,CHEQUE,7f78bd65-017e-4337-9459-7e5dfa447d66,
               Salary,Ipayroll Limite,Acme Inc,,Ipayroll,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,4e6bad3b-89c7-458c-a993-38859da68b54,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,55f94f77-601b-4808-ab38-b71cd5f4d6f7,
               Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,144.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,1c4d9d4c-46de-4c39-a2f4-293beb646cc8,
               Chq/Withdrawal,,0894T0129180,,,-550.00,2012-08-16T00:00:00.0000000,SURPLUS,CHEQUE,780f11dd-5ce4-4562-837c-f6043bdca27f,
               Credit Card Debit,Charcoal Burger          Thames   Nz,4367-****-****-3239,,,-27.50,2012-08-17T00:00:00.0000000,SURPLUS,VISA,136e9010-9d07-4ddb-b7a3-abd209a23f44,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-14.00,2012-08-17T00:00:00.0000000,SURPLUS,VISA,17ef7740-7714-467a-be8e-32089268d1d0,
               Credit Card Debit,Mcdonalds Queen        Auckland     Nz,4367-****-****-3239,,,-8.50,2012-08-18T00:00:00.0000000,SURPLUS,VISA,1d207cd2-48ca-44b7-b0ea-364ed73ffb75,
               Credit Card Debit,Paknsave Thames      Thames   Nz,4367-****-****-3239,,,-224.80,2012-08-19T00:00:00.0000000,FOOD,VISA,5dddf943-5c93-4e8f-b928-ecd0ceb14af5,
               Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-23.50,2012-08-19T00:00:00.0000000,SURPLUS,VISA,2af5eaf4-9bed-49b0-8463-0fbca12d7cdc,
               Credit Card Debit,Placemakers Mt Wgtn      Auckland     Nz,4367-****-****-3239,.,,-76.96,2012-08-19T00:00:00.0000000,SURPLUS,VISA,58dd11a6-2ff1-4e85-9948-56090d47cb27,
               Credit Card Debit,Paknsave Fuel Thames Thames   Nz,4367-****-****-3239,,,-78.63,2012-08-19T00:00:00.0000000,FUEL,VISA,c8049b2e-2662-48aa-b407-762269ff42bf,
               Payment,The Very Big Telco,202610963,095501328,B Smith,-91.98,2012-08-20T00:00:00.0000000,PHNET,CHEQUE,56b22788-d1e6-4a5d-a715-08b563dae678,
               Payment,The very cool Power Co,A B Smith,659792,Energyonline,-212.07,2012-08-20T00:00:00.0000000,POWER,CHEQUE,17b8c91c-5174-41a7-b382-b6a9ff52a11a,
               Credit Card Debit,Z Queen Street            Auckland     Nz,4367-****-****-3239,,,-28.49,2012-08-20T00:00:00.0000000,FUEL,VISA,bd12376a-6aac-4171-a23c-f85af168582f,
               """";
    }

    public static TransactionSetDto TransactionSetDtoTestData1()
    {
        //    "VersionHash,15955E20-A2CC-4C69-AD42-94D84377FC0C,TransactionCheckSum,6704009528804600641,2012-08-20T00:00:00.0000000                                                    ",
        //    "Direct Credit,Smith A B & J S,,,,3474.02,2012-08-15T00:00:00.0000000,PAYCC,CHEQUE,7f78bd65-017e-4337-9459-7e5dfa447d66,                                               ",
        //    "Salary,Ipayroll Limite,Acme Inc,,Ipayroll,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,4e6bad3b-89c7-458c-a993-38859da68b54,                                       ",
        //    "Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,3000.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,55f94f77-601b-4808-ab38-b71cd5f4d6f7,                     ",
        //    "Direct Credit,Acme Inc Ltd      Dl,Salary,Acme Inc,Acme Inc,144.00,2012-08-15T00:00:00.0000000,INCOME,CHEQUE,1c4d9d4c-46de-4c39-a2f4-293beb646cc8,                      ",
        //    "Chq/Withdrawal,,0894T0129180,,,-550.00,2012-08-16T00:00:00.0000000,SURPLUS,CHEQUE,780f11dd-5ce4-4562-837c-f6043bdca27f,                                                 ",
        //    "Credit Card Debit,Charcoal Burger          Thames   Nz,4367-****-****-3239,,,-27.50,2012-08-17T00:00:00.0000000,SURPLUS,VISA,136e9010-9d07-4ddb-b7a3-abd209a23f44,      ",
        //    "Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-14.00,2012-08-17T00:00:00.0000000,SURPLUS,VISA,17ef7740-7714-467a-be8e-32089268d1d0,      ",
        //    "Credit Card Debit,Mcdonalds Queen        Auckland     Nz,4367-****-****-3239,,,-8.50,2012-08-18T00:00:00.0000000,SURPLUS,VISA,1d207cd2-48ca-44b7-b0ea-364ed73ffb75,     ",
        //    "Credit Card Debit,Paknsave Thames      Thames   Nz,4367-****-****-3239,,,-224.80,2012-08-19T00:00:00.0000000,FOOD,VISA,5dddf943-5c93-4e8f-b928-ecd0ceb14af5,            ",
        //    "Credit Card Debit,The Video Shop           Thames Aknz,4367-****-****-3239,,,-23.50,2012-08-19T00:00:00.0000000,SURPLUS,VISA,2af5eaf4-9bed-49b0-8463-0fbca12d7cdc,      ",
        //    "Credit Card Debit,Placemakers Mt Wgtn      Auckland     Nz,4367-****-****-3239,.,,-76.96,2012-08-19T00:00:00.0000000,SURPLUS,VISA,58dd11a6-2ff1-4e85-9948-56090d47cb27, ",
        //    "Credit Card Debit,Paknsave Fuel Thames Thames   Nz,4367-****-****-3239,,,-78.63,2012-08-19T00:00:00.0000000,FUEL,VISA,c8049b2e-2662-48aa-b407-762269ff42bf,             ",
        //    "Payment,The Very Big Telco,202610963,095501328,B Smith,-91.98,2012-08-20T00:00:00.0000000,PHNET,CHEQUE,56b22788-d1e6-4a5d-a715-08b563dae678,                            ",
        //    "Payment,The very cool Power Co,A B Smith,659792,Energyonline,-212.07,2012-08-20T00:00:00.0000000,POWER,CHEQUE,17b8c91c-5174-41a7-b382-b6a9ff52a11a,                     ",
        //    "Credit Card Debit,Z Queen Street            Auckland     Nz,4367-****-****-3239,,,-28.49,2012-08-20T00:00:00.0000000,FUEL,VISA,bd12376a-6aac-4171-a23c-f85af168582f,    "
        var set = new TransactionSetDto
        (
            StorageKey: "Gooo",
            LastImport: new DateTime(2013, 8, 15),
            Transactions:
            [
                new TransactionDto(
                    TransactionType: "Direct Credit",
                    Description: "Smith A B & J S",
                    Amount: 3474.02M,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.PayCreditCardBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("7f78bd65-017e-4337-9459-7e5dfa447d66")
                ),
                new TransactionDto(
                    TransactionType: "Salary",
                    Description: "Ipayroll Limite",
                    Reference1: "Acme Inc",
                    Reference2: "",
                    Reference3: "Ipayroll",
                    Amount: 3000M,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("4e6bad3b-89c7-458c-a993-38859da68b54")
                ),
                new TransactionDto(
                    TransactionType: "Direct Credit",
                    Description: "Acme Inc Ltd      Dl",
                    Reference1: "Salary",
                    Reference2: "Acme Inc",
                    Reference3: "Acme Inc",
                    Amount: 144M,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("55f94f77-601b-4808-ab38-b71cd5f4d6f7")
                ),
                new TransactionDto(
                    TransactionType: "Direct Credit",
                    Description: "Acme Inc Ltd      Dl",
                    Reference1: "Salary",
                    Reference2: "Acme Inc",
                    Reference3: "Acme Inc",
                    Amount: 3000,
                    Date: new DateOnly(2012, 08, 15),
                    BudgetBucketCode: TestDataConstants.IncomeBucketCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("1c4d9d4c-46de-4c39-a2f4-293beb646cc8")
                ),
                new TransactionDto(
                    TransactionType: "Chq/Withdrawal",
                    Description: "",
                    Reference1: "0894T0129180",
                    Reference2: "",
                    Reference3: "",
                    Amount: -550M,
                    Date: new DateOnly(2012, 08, 16),
                    BudgetBucketCode: SurplusBucket.SurplusCode,
                    Account: TestDataConstants.ChequeAccountName,
                    Id: new Guid("780f11dd-5ce4-4562-837c-f6043bdca27f")
                ),
                new TransactionDto(
                    TransactionType: "Credit Card Debit",
                    Description: "Charcoal Burger          Thames   Nz",
                    Reference1: "0894T0129180",
                    Reference2: "4367-****-****-3239",
                    Reference3: "",
                    Amount: -27.50M,
                    Date: new DateOnly(2012, 08, 17),
                    BudgetBucketCode: SurplusBucket.SurplusCode,
                    Account: TestDataConstants.VisaAccountName,
                    Id: new Guid("136e9010-9d07-4ddb-b7a3-abd209a23f44")
                )
            ]
        );

        return set;
    }
}
