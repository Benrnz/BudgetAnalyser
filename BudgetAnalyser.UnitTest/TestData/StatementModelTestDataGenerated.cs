using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class StatementModelTestDataGenerated
    {
        public static IAccountTypeRepository AccountTypeRepo { get; set; }
        public static IBudgetBucketRepository BudgetBucketRepo { get; set; }

        /// <summary>THIS IS GENERATED CODE </summary>
        [GeneratedCode("StatementModelTestDataGenerator.GenerateCSharp", "11/23/2015 13:04:40")]
        public static StatementModel TestDataGenerated()
        {
            var model = new StatementModel(new FakeLogger())
            {
                StorageKey = @"C:\Foo\StatementModel.csv",
                LastImport = new DateTime(2015, 11, 21)
            };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -9.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 20),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7a8cc7f2-c257-4d41-9b4c-87e4c623f482"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 20),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("110a6497-b19b-4faf-a932-7a41312b0f61"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -6.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 20),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("46e45965-275f-415f-a49b-bcd0031541c0"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -11.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 21),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("b0438bb3-112d-40c2-81c0-29590e00853c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -55.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 10, 21),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ce628396-3f6b-4980-88ff-e4ea68a5c804"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -24.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 10, 21),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("67f9cbff-18ca-4d11-a5ef-22988e04584a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -20.81M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("POWER"),
                    Date = new DateTime(2015, 10, 21),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("d60f66aa-1120-49e0-b2fa-e63852d62dd9"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -34.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 22),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("a8f49deb-f932-44f4-8c8e-afbabfdbfa84"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -3000.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("c66eb722-6d03-48b2-b985-6721701a01ae"),
                    Reference1 = "automatchref12",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -564.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("INSLIFE"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("24b6f198-898e-40ae-b12d-388fd0daa472"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -130.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("233a8a87-5583-47ef-ae72-592aa9c10eb8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -114.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("INSCAR"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2bfa0126-09ab-4583-84ef-05d2ea756d4c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -102.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("INSHOME"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("9ead17b2-8978-411b-91a4-62a427eaee21"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -68.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("REGO"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("786b98fe-536d-46b4-b57b-5b82c1e985da"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -20.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ea9ae6a2-c9d2-4238-b7cf-7b40ee77bb72"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -32.41M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("79a48868-7160-4b6f-9883-60156dd1d6e2"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -27.60M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("6838efc5-872d-449a-9e29-3425beba7c79"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },

                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 3000.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("9ecd767b-b3c7-4c15-a40f-1c0098e422ed"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 564.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("INSLIFE"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2793f4c6-b37d-4bfb-beed-4f817dc8fc58"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 130.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("1a0dd7ef-2962-4c8c-85cb-7d3e64748211"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 114.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("INSCAR"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("42735463-fb9b-49f4-8a26-936a1cc20e9a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 102.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("INSHOME"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e86b5e4d-fad0-4255-9ce7-e682197b64d6"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 68.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("REGO"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("b1e56e0f-a96f-4364-ad73-77f88b6b31c7"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 20.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 10, 23),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("d743ebc1-1697-45e1-870d-81043142ce43"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -275.43M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 10, 25),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8b008836-3f35-4095-aeb4-b907765ffb56"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -15.99M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 25),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("56e3bc85-30f4-47c5-9782-f3a16ad69a93"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -6.20M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 25),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("bd57bac2-08f8-4036-98b1-cd23eb2b0e6e"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -12.70M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 25),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("08c8e941-aefd-4a13-b492-dcfffaefa35a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -58.93M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CLOTHES"),
                    Date = new DateTime(2015, 10, 26),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8ef4cc4a-efe3-49d9-a0bf-735ac435a761"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -17.25M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 26),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f2003ef7-2b15-448a-82cb-6608b52a8a76"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -152.30M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 10, 26),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("bed39dfb-26bd-45eb-bab1-c47bb3d534ed"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -39.96M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 10, 26),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e590d0a8-9e27-444f-9d55-c07dff41d3e1"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -29.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 26),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("05f5007e-1094-4ffe-8068-edd1abc030e8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -60.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 27),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("aed79385-473e-4384-988c-643bc6bb1bce"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Atm Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -27.03M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 10, 27),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("6a104e00-ee6a-4530-a488-20a9e63c2bc4"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -39.20M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 10, 27),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("5cc62085-f7fd-48bc-a5e0-495cb3d8b2ce"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -23.96M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 10, 28),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("90b7cce0-baf0-4258-943b-1e3ffd25f15e"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -11.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 29),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("9afd8670-1f8b-48e2-af1d-d6dad513b6fd"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -92.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 29),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("4e746665-37ce-47da-ae78-13d5adfc2ab2"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -3.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 29),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e447b974-8da5-4200-ae15-7eaaf3e6831c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -10.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("cf2f7845-ef16-4156-8bd5-f9fd19d3180c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -22.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("a76111b9-86b4-4f88-ab57-0e04f52c9888"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -25.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8c79342c-1ac2-49e0-9a3a-693a44f0ab73"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -12.80M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("c6b41c5a-153b-4ae5-afc2-0abffbcbc4a5"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -35.48M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("94f3fc23-57ec-4924-96fb-43cac64d2a54"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -7.99M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8a7ea9dd-e654-49b3-a128-8a6aa7d86002"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 14.01M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("a72104a8-f62b-4ec7-90f0-5c47e8b5fb07"),
                    Reference1 = "",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Interest Paid")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = -4.62M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 10, 30),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("231f1731-666c-47bc-a76f-57810e97a809"),
                    Reference1 = "",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Withholding Tax")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -24.22M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("60412b90-dfc5-4b3b-844f-6fb73a60c688"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -242.28M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2eca888e-a3f9-497b-a341-3b6f723afb6b"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -27.44M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FUEL"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7ac24965-e21e-4f63-b3e4-9ec391843f56"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -24.80M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("4936ef28-1d2d-4395-93dc-2dd8bd84b2a3"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -73.34M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 10, 31),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7a9dd17a-1957-4ac4-8175-28a93d9a1eb9"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -32.70M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 1),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("31b7af5c-4429-4cf7-b0b4-54ff8d72eb1d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -211.74M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 1),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("6d7196ce-0049-451f-af25-f1f2117aa2c9"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -30.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FUEL"),
                    Date = new DateTime(2015, 11, 1),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("43a74e7d-e8a7-4d4c-af1a-e9d710edf691"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -49.70M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 1),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("246fe376-d6b2-4a09-a881-f07d1577ffa7"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -25.49M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("POWER"),
                    Date = new DateTime(2015, 11, 2),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("4292f892-7f62-43dd-b224-275e53cbdc05"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -30.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 2),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("886b4bd5-455c-42bb-826c-cc61edcec84f"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Atm Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -120.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 2),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8673ce8f-629a-413f-b7c9-afbcc123cf80"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Atm Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = 2650.98M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 3),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("dc7ab7ca-ea4d-453a-9051-320e5b328b57"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Direct Credit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -2.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("DOC"),
                    Date = new DateTime(2015, 11, 3),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("5c70f5b2-632f-49f0-bfb5-3e95a19fbe2d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -849.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 3),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("04f9f607-e6cc-449b-a18b-a5b45a7a1e2a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -10.79M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 3),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2a3b17cf-c4a0-4c01-893b-73677cc650ad"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -2650.98M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 4),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("bace1969-e4d4-4374-8cef-e671e1287b6c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Debit Transfer")
                },

                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("SAVINGS"),
                    Amount = 2650.98M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SAVINGS"),
                    Date = new DateTime(2015, 11, 4),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("1d64da2c-6f87-4016-90a6-f7dd0baaccef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Credit Transfer")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.30M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 5),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("2a5b1821-802d-4b5e-af72-728dde935931"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.20M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 5),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("0ec50bda-e175-495b-86da-8b21d5b3cbf8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -7.99M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 5),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("b87d9668-6d17-444f-89ef-d53cd1ba826c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -17.32M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("POWER"),
                    Date = new DateTime(2015, 11, 6),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("af952e91-9ca8-442e-9569-13b8796c9eef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -42.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 6),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("00172bd9-0ec4-4d92-94bf-d533299e0dc5"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -30.60M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 6),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("63a9d1dd-a28e-4561-8443-570af6f1da6a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -135.93M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("d889725b-5e4b-42d6-8528-27da7624a77a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -11.47M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e53f7c07-f06b-4a99-8310-57d6f7aef2e8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -118.98M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("bab0a41a-4b78-4277-ae73-2392a2ab5f75"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -31.49M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("810c31e3-bdfb-4aef-a6dd-b96a3789e27f"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -14.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 8),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("34249540-ac64-4392-9fa7-c0e10ecc06af"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -2.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 9),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f705a9c8-533c-451e-9e4b-70266679af58"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -16.59M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("DOC"),
                    Date = new DateTime(2015, 11, 10),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("a93e7522-7009-464e-b82f-9ac989564c96"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -77.70M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FUEL"),
                    Date = new DateTime(2015, 11, 10),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f74050f4-3825-4ed6-81c7-0b6355c9f16e"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -60.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("DOC"),
                    Date = new DateTime(2015, 11, 10),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("592c962d-7864-49eb-b90e-6def768f355b"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -4.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 10),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("6c225cae-970f-4c33-9c01-9a2d4d4a0cb3"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -26.74M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FUEL"),
                    Date = new DateTime(2015, 11, 11),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("a0158fe7-c26c-46fe-b6ef-49cd9b9e6b82"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -38.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 11),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("b56e8c82-501e-4c77-b2ba-0fa9e86e19ef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -4.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 11),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7dcb2571-7d35-457f-96c3-61e5fcfde803"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -5.95M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("POWER"),
                    Date = new DateTime(2015, 11, 12),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("34720b1b-f39a-4198-9c4b-abf619636acc"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 12),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("eb465d02-e82c-4d5d-884a-2adb1c86cf4c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -9.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 12),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("0ec1c6b3-8f4c-47d5-99e2-c01168cbae2c"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -40.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ac3a08f2-3274-4667-b382-9f23e81fa9e8"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -1618.60M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("DAYCARE"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f6169a00-165e-4607-857e-79fa9e82d565"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -444.63M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ce618893-06ab-4dab-8fd8-5578e6b90700"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -11.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 13),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("5db38e40-a58b-47bb-9b1c-077b5f8cecc5"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -225.24M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("3cbbfe15-b62c-422b-b1da-d07265b5d166"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -55.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("HAIRCUT"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("ce59eff8-61c8-42bc-8bc6-1bf22efdb5ba"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -159.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS.FENCE"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("92a6d37b-a846-4b2c-b9b3-14ba85c70295"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -24.10M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("f32b2ed3-717f-41b5-8892-d48df86579ef"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -471.01M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("CAR MTC"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8626f06a-5ba6-45e1-b197-4c0b0ab927c3"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -8.64M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 14),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("0f3d74ba-4456-41a1-a58e-b006534b664b"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -141.79M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 15),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("15bde657-e077-42a3-b0eb-1b49f76ba17d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -100.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("RABO"),
                    Date = new DateTime(2015, 11, 16),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("7c21ded9-1e06-4533-9e1b-d3ddefb139a0"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Automatic Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -120.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("POWER"),
                    Date = new DateTime(2015, 11, 16),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("1c75d1ea-f758-4450-8d8b-6c549a726c94"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },

                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -5.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 16),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("860cce1e-abca-4520-8a03-6e73a2ddca13"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -54.80M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("WATER"),
                    Date = new DateTime(2015, 11, 17),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e77a63a0-e156-4b4d-aec2-5f76d5c7a166"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -8.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 18),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8a6cd650-77cc-427b-a481-ac4b286ccfd4"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Eft-Pos")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -66.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("DOC"),
                    Date = new DateTime(2015, 11, 18),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("e2bb8651-331c-46c8-8c5c-c4afc1bae2f2"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -6.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 18),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("72538c59-a4fc-4643-8bc7-f7c39a6190eb"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -5.00M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("DOC"),
                    Date = new DateTime(2015, 11, 18),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("870b06b4-db52-4387-bebd-a513bcde37f0"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -2961.51M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PAYCC"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("c9613f78-fbd5-46e2-b452-267f9a26755e"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("One-Off Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -27.74M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("840acbce-af64-4ce8-a3e9-d33beea48bdb"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Payment")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("CHEQUE"),
                    Amount = -25.30M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("PHNET"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("9e18b22f-9623-44e7-b391-e868b482d545"),
                    Reference1 = "5235ghkh",
                    Reference2 = "792fgjgghkh",
                    Reference3 = "kjhgjklshgjsh",
                    TransactionType = new NamedTransaction("Payment")
                },
 

                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -45.98M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("212a5450-81c7-4441-a851-84a1830e7a4a"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -3.50M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("aeb3c1ac-fb5a-4164-8125-3b5cf3487616"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -7.80M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("FOOD"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("8c1ecc46-ca7b-4421-8bfa-c41e3d4744d3"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                },
                new Transaction
                {
                    Account = AccountTypeRepo.GetByKey("VISA"),
                    Amount = -31.90M,
                    BudgetBucket = BudgetBucketRepo.GetByCode("SURPLUS"),
                    Date = new DateTime(2015, 11, 19),
                    Description = "Lorem Ipsum Dolor",
                    Id = new Guid("02861f41-a92c-4ffd-bd22-8194dc402f8d"),
                    Reference1 = "5235ghkh",
                    Reference2 = "",
                    Reference3 = "",
                    TransactionType = new NamedTransaction("Credit Card Debit")
                }
            };

            return model.LoadTransactions(transactions);
        }
    }
}