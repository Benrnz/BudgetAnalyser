using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class MatchingRulesTestDataGenerated
    {
        public static IBudgetBucketRepository BucketRepo { get; set; }

        /// <summary> THIS CODE IS GENERATED  </summary>
        [GeneratedCode("MatchingRulesTestData.ConvertToDomainAndGenerateCSharp", "1")]
        public static IEnumerable<MatchingRule> TestData1()

        {
            return new List<MatchingRule>

            {
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FEES",
                    Created = new DateTime(2014, 4, 12),
                    Description = null,
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 4,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("f21881b2-4ca7-4d90-b2f5-4d08a56b38cc"),
                    TransactionType = "Other Bank ATM Fee",
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "MTG",
                    Created = new DateTime(2014, 4, 23),
                    Description = null,
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 21,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = "004600",
                    RuleId = new Guid("07764030-c1db-4db1-870b-45f3c2bd44b3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 2, 2),
                    Description = "1920'S Cafe              Auckland     Nz",
                    LastMatch = new DateTime(2014, 2, 2),
                    MatchCount = 8,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("d6a25f8b-676c-4e67-afc7-c934ac141c23"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.PhoneBucketCode,
                    Created = new DateTime(2014, 4, 20),
                    Description = "2 Degrees Ben",
                    LastMatch = new DateTime(2014, 5, 18),
                    MatchCount = 10,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("eb019cc4-20b1-4f96-bd2d-e310de8c1492"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.PhoneBucketCode,
                    Created = new DateTime(2014, 4, 28),
                    Description = "2Degrees Ness",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 9,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ca9f10a0-6a7d-4604-a743-e52130675d2b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "JOURNAL",
                    Created = new DateTime(2014, 4, 23),
                    Description = "4367-****-****-3221",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 10,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("a8153c89-aeb8-4955-9ed3-6a847fe45f39"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 20),
                    Description = "Abbotts Way Veterinary   Remuera      Nz",
                    LastMatch = new DateTime(2014, 3, 20),
                    MatchCount = 4,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("66d1e5c0-6422-48b7-a311-413adfda6550"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 6),
                    Description = "Ajadz Indian Cuisine     Ellerslie    Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 4,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("1041b203-ae27-44ba-938b-1b6277e86071"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 5, 3),
                    Description = "Ajadz Indian Cuisine   Ellerslie     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("c9a4c8b8-9100-4f49-b433-07a7629d1841"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLEANER",
                    Created = new DateTime(2014, 4, 28),
                    Description = "Anz  S3B1592 Fix - 4 Fort",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 12,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("1e1807a9-e5a5-4778-b182-70d57bd1be0b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "INSHOME",
                    Created = new DateTime(2014, 4, 23),
                    Description = "Aon-Nz Dds-Fidu",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 7,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("6d8b57bf-9389-4d17-a223-80181cab648e"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.RatesBucketCode,
                    Created = new DateTime(2014, 2, 26),
                    Description = "Auckland City",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8b1d17fb-74cc-4984-bff5-bf7276660d99"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.PowerBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Auckland Energy Cons",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("c91b2d78-9fe2-4d79-923b-76cba07717a3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 6),
                    Description = "Bakers Delight Pakuranga Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("0ecefc1f-450d-4e83-9964-a7f344c5a962"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DAYCARE",
                    Created = new DateTime(2014, 4, 23),
                    Description = "Bear Park Kohi",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 5,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("f61ee830-fd34-4541-ac35-87447989933f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DAYCARE",
                    Created = new DateTime(2014, 4, 23),
                    Description = "Bear Park Kohi",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = "Sophierees",
                    RuleId = new Guid("f6dda1d0-84e4-4122-bbf9-aa033b8e7524"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 12),
                    Description = "Ben                      Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 8,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("69e6fb9d-a890-405d-bb99-e99112b1265b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 28),
                    Description = "Ben                    Auckland      Nz",
                    LastMatch = new DateTime(2014, 4, 28),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("92587ef0-e61b-4432-85a2-01aa027f6200"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 3, 11),
                    Description = "Bendon                   Auckland Airpnz",
                    LastMatch = new DateTime(2014, 3, 11),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("3c65b496-2866-4a57-8174-b71d4c0ecbd7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 21),
                    Description = "Berkeley Mission Bay     Mission Bay  Nz",
                    LastMatch = new DateTime(2014, 1, 21),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("3c3a3f52-59e6-4547-b1a1-dccff99ec988"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 12, 2),
                    Description = "Bp 2 Go Ellerslie        Auckland     Nz",
                    LastMatch = new DateTime(2013, 12, 2),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("de0ff4c6-788b-4421-ba1f-de146bdad9a0"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 12, 29),
                    Description = "Bp 2Go Meadowbank        Auckland     Nz",
                    LastMatch = new DateTime(2013, 12, 29),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("7c3408cb-2b8f-444c-9811-a1d2614be06d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 3, 11),
                    Description = "Bp Oil New Zealand L     Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 11),
                    MatchCount = 6,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("319e7a15-a1a7-41e4-8201-d301da295a74"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 23),
                    Description = "Bp Oil New Zealand L   Auckland      Nzl",
                    LastMatch = new DateTime(2014, 4, 23),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8534d890-f1e1-44c9-a5c4-58af3785c1e8"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 12),
                    Description = "Brew On Quay             Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 8,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("68fb5539-33df-449b-8d6c-2325e6a99f30"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 2, 21),
                    Description = "Briscoes Panmure         Mt Wellingtonnz",
                    LastMatch = new DateTime(2014, 2, 21),
                    MatchCount = 7,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("617e9bb9-de71-47e8-8ddf-1dc65b106a9b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Caltex Abbotts Way       Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e000aa36-1141-471d-8a36-61c23a431847"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 11, 16),
                    Description = "Caltex Bombay            Bombay       Nz",
                    LastMatch = new DateTime(2013, 11, 16),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4921679b-5c51-4878-a911-77d4ac7f2164"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 6),
                    Description = "Caltex Penrose           Penrose      Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 6,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bc433ff6-f439-4f14-8d6f-5b2f2b8af370"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Caltex St Heliers        St Heliers   Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("7a43b2d8-e4b5-4758-834f-2dc058871f54"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 8),
                    Description = "Charcoal Burger          Meadowbank   Nz",
                    LastMatch = new DateTime(2014, 3, 8),
                    MatchCount = 7,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("c03c786e-0b87-4f82-996d-4ef2a127a912"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "WASTE",
                    Created = new DateTime(2014, 2, 26),
                    Description = "Clippa Bags",
                    LastMatch = new DateTime(2014, 5, 18),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8a316a0b-a36d-4312-b0a2-dba5ba2b2f1d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 21),
                    Description = "Columbus Coffee Sylv     Auckland     Nz",
                    LastMatch = new DateTime(2014, 1, 21),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("7306438b-114f-47fe-bc4e-a2736ca18e3a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 5, 8),
                    Description = "Columbus Coffee Sylv   Auckland      Nzl",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4b1ec425-9c9f-4f08-8dff-1ecfed5216e8"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 12),
                    Description = "Countdown                Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 59,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e3921f2f-6560-442a-82f3-af7b2a14fd44"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 28),
                    Description = "Countdown              Auckland      Nzl",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 13,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("5be599a1-9c1a-4d4c-81dd-94616ddfb68d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 1, 21),
                    Description = "Countdown - Pakurang     Auckland     Nz",
                    LastMatch = new DateTime(2014, 1, 21),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("269b8539-c2b5-4547-8bfc-02225d853f66"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 2, 26),
                    Description = "Countdown Meadowbank",
                    LastMatch = new DateTime(2014, 2, 26),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("03fdb92d-9fcc-4bab-a3c4-d5c7e5e728df"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Countdown Sylvia Par     Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("03175aa9-0f48-4b0e-b746-2d05006d2a0a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2013, 10, 19),
                    Description = "Cranwells Pharmacy       Remuera      Nz",
                    LastMatch = new DateTime(2013, 10, 19),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("11538a51-a787-4db8-b22e-cfeca7830580"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Created = new DateTime(2013, 10, 19),
                    Description = "Cyclespot Spare Parts    North Shore  Nz",
                    LastMatch = new DateTime(2013, 10, 19),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("435c1312-7dfc-4a8a-a5dd-039852135804"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 21),
                    Description = "De Fontein Belgian Beer  Mission Bay  Nz",
                    LastMatch = new DateTime(2014, 1, 21),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("77a03ab8-d9c2-408b-9fa6-6923a23ee118"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Doolan Brothers Newmarketauckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("eb7b9404-8daf-4339-9d20-c414d5bad0d9"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 2, 21),
                    Description = "Dorchester Superette",
                    LastMatch = new DateTime(2014, 2, 21),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("027cb533-40a1-452c-af63-def501dfbded"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 12),
                    Description = "Dorchester Superette     Meadowbank   Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("339d7c86-14f9-4cf8-bb70-d3948ef6dc8f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 5, 3),
                    Description = "Ellerslie Best Pizza L Auckland      Nzl",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("6d935db8-6e22-44d9-a0c3-faf862e46933"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.PowerBucketCode,
                    Created = new DateTime(2014, 4, 23),
                    Description = "Energy Online",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 11,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = "Energyonline",
                    RuleId = new Guid("a2ce2096-1b7f-4397-9ae7-b1a4e112176d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Esquires Gz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("7c0e1d7b-6f0f-45b2-aaeb-09de9d2761a0"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Ezibuy International     Parnell      Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ca2a5a3d-fbdd-4209-86c9-bc823f842664"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Farro Fresh              Mt Wellingtonnz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("f9ce14ab-d6f3-413a-9feb-2e86c22f6c79"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 8),
                    Description = "Frolic Cafe              Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 8),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bae27f5c-7246-4457-b90f-86a44312b4ab"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Garrison Public House    Mt Wellingtonnz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("6aae5d7c-71bd-4231-9263-f25b2a793e64"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Gentle Chiropractic Co   Meadowbank   Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("6315aec4-084c-49c9-969e-4850a2b76047"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 4, 23),
                    Description = "Glassons Sylvia Park     Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 23),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ed2ab707-1d77-4564-8b18-224edf210040"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 5, 8),
                    Description = "Glassons Sylvia Park   Auckland      Nzl",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("019364b3-3b88-4499-be95-67600ec9a622"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 21),
                    Description = "Gourmet Pizzeria         Auckland     Nz",
                    LastMatch = new DateTime(2014, 1, 21),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("a60d5a7c-4930-404e-a1c8-3aee1e3b6dff"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Gull Reeves Rd 311     Pakuranga     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("96c4c5e3-fbd3-402a-99f8-f29882f57fe3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Hallensteins Sylvia      Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("a6d2bf5c-6b00-4c79-a57e-abed0ef24538"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2013, 11, 9),
                    Description = "Hell Pizza Ellerslie     Auckland     Nz",
                    LastMatch = new DateTime(2013, 11, 9),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("817a1e36-38bf-4159-88cb-23127da0b05a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Hell Pizza Kohimarama    Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e39a2bb0-7f75-4db3-bc52-eaea286921c4"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 2),
                    Description = "Hollywood Bakery Pakurangpakuranga    Nz",
                    LastMatch = new DateTime(2014, 1, 2),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("1e6584cc-5b8b-4af3-bd21-6a23bf04ec26"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Hoyts Botany Downs       Botany Downs Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bb66674d-3531-45e9-848d-2f400f2ebdfc"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 13),
                    Description = "Hoyts Sylvia Park        Mt Wellingtonnz",
                    LastMatch = new DateTime(2014, 1, 13),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("3c7b91db-24b7-432f-ac8f-9ba03e23bc1c"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "ISALARY",
                    Created = new DateTime(2014, 4, 20),
                    Description = "Ipayroll Limite",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 11,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("abf3c5f8-f222-44e9-856c-b132ef7ccede"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.CarMtcBucketCode,
                    Created = new DateTime(2014, 2, 8),
                    Description = "John Andrew Ford & M",
                    LastMatch = new DateTime(2014, 2, 8),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("76675b63-a92b-4e20-8b72-ab298ba726a7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2013, 11, 16),
                    Description = "Just Jeans               Mt Wellingtonnz",
                    LastMatch = new DateTime(2013, 11, 16),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("88c91fa1-8542-4365-b8fd-42421a2ae665"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Kahve                    St Heliers Aunz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("b9d8e4c6-fbfd-4a25-9508-a1fe9b6daad2"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 28),
                    Description = "Kreem Cafe               Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 28),
                    MatchCount = 7,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ba183ce8-0e31-45a5-b99e-9bf80c516f9c"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "La Fourchette            Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bdc555cf-4801-4afb-ac15-0c83262f4dd3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "GYM",
                    Created = new DateTime(2014, 2, 8),
                    Description = "Les Mills Brito",
                    LastMatch = new DateTime(2014, 2, 8),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("460831da-4642-4c3a-9f8b-87f9239fdcce"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Life Pharmacy Sylvia Pk  Sylvia Park  Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("84b8ac5e-bb4e-4cd6-80a1-c5fe50395e9f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Long Tall Sally          London  Ec1V Gb",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("35aac5cf-9522-4063-a55b-8b4e09e8f22b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2013, 11, 9),
                    Description = "Mac Roast",
                    LastMatch = new DateTime(2013, 11, 9),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("0abafabe-40c0-48f6-8eb6-abf3998de1d4"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 6),
                    Description = "Mahuhu Espresso          Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 12,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("2d874431-03f0-45ba-9df1-7a30656e4329"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 3, 11),
                    Description = "Max Fashions             Pakuranga    Nz",
                    LastMatch = new DateTime(2014, 3, 11),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ba2ebb3c-0547-447b-acea-8bbad61448ed"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 4, 23),
                    Description = "Max Fashions             Sylvia Park  Nz",
                    LastMatch = new DateTime(2014, 4, 23),
                    MatchCount = 5,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("920622eb-c0c6-453a-9306-71d69a133236"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 6),
                    Description = "Mcdonalds Lunn Ave       Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 7,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("992bdf65-eb2b-4cee-bdf9-665cc1dea79f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 5, 3),
                    Description = "Meadowbank Amcal Pharm Meadowbank    Nzl",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("3eb315df-05ae-4775-8b1a-d01cc6356b1c"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 12),
                    Description = "Meadowbank Amcal Pharmacymeadowbank   Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 5,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("42dd174d-895d-419e-984f-1a5954993f4a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 3, 20),
                    Description = "Meadowbank Bakery        Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 20),
                    MatchCount = 5,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("9eb78910-422d-4733-a9ca-e8a7cba691f9"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Meadowbank Bakery      Auckland      Nz",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4159747c-8994-4fae-bbd8-63bdc98cd205"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Meadowbank Dental        Saint Johns  Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("05f4afce-afc1-4bfd-a5dd-e3a81e2c0ef3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 20),
                    Description = "Meadowbank Paper Plu",
                    LastMatch = new DateTime(2014, 3, 20),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("49e520a2-e8aa-4b19-9e10-26d1784d49b2"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 28),
                    Description = "Meadowbank Roast",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 16,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("f2ddedab-bcdd-4b86-8a0f-3fe0fb1725df"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 28),
                    Description = "Meadowbank Wines & Spi   St Johns     Nz",
                    LastMatch = new DateTime(2014, 3, 28),
                    MatchCount = 7,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("99c0277d-f191-46ef-9648-40f194ff969d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2013, 12, 2),
                    Description = "Mitre 10 Mega Mt Wtgn    Mount Wellingnz",
                    LastMatch = new DateTime(2013, 12, 2),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("66023831-fa0b-4663-aa1e-56538149b2d6"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 2, 8),
                    Description = "Mobil Clearview          St Heliers   Nz",
                    LastMatch = new DateTime(2014, 2, 8),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("9f78d269-69db-4694-b67b-537db0925e80"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 5, 3),
                    Description = "Mobil Clearview        Auckland      Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("32478a58-af79-435e-af00-9b1ad5256cbc"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Mobil Greenlane          Peach Parade Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("1c634075-1186-4e0d-a41b-6fbaafbe9801"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 10, 19),
                    Description = "Mobil Quay Street        Auckland     Nz",
                    LastMatch = new DateTime(2013, 10, 19),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("93631964-456e-4317-bdd8-e92a1c1e19e7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Msft   *Windows Store    Bill.Ms.Net  Sg",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("2131c21a-a8c2-43e2-9424-b1aba09cd467"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Muffin Break Botany      Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("91f24599-90ca-4da5-8ddf-e0661ac84dae"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 12),
                    Description = "Muffin Break Pakuranga   Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("be12a3c7-1972-4777-8a0d-4d4a7bf004a0"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "GIFT",
                    Created = new DateTime(2014, 4, 29),
                    Description = "My Refund Limited",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("591e2ecd-9ead-4d9d-9823-20de53a0ef8d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2013, 12, 7),
                    Description = "New World Eastridge      Auckland     Nz",
                    LastMatch = new DateTime(2013, 12, 7),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("d86ef7c4-b25e-4cda-a04a-894777dab9bf"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 1, 2),
                    Description = "New World Stonefields    Auckland Nz  Nz",
                    LastMatch = new DateTime(2014, 1, 2),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("97239a31-7ff5-4308-98c1-90b80990f7d7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Nosh Gourmet Food Market Warkworth    Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("50b9985d-fae5-4d32-976d-0301028c60f7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.RegoBucketCode,
                    Created = new DateTime(2014, 3, 20),
                    Description = "Nz Transport Agency      Palm North   Nz",
                    LastMatch = new DateTime(2014, 3, 20),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("de8d2f41-9050-4e5f-9383-4c207a94bb23"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 3, 20),
                    Description = "Nzta Tolling             069536343    Nz",
                    LastMatch = new DateTime(2014, 3, 20),
                    MatchCount = 6,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bfeb78f1-2aee-4435-a231-1b92ec7b892f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Occidental Belgian Beer  Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("436539f7-c16f-43a0-904a-f1cc2a10702b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "INSLIFE",
                    Created = new DateTime(2014, 2, 26),
                    Description = "One Path Insurance",
                    LastMatch = new DateTime(2014, 2, 26),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("fc2ed6a1-2e8f-414b-be5b-8426484efead"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 6),
                    Description = "Pak N Save Glenn Innes   Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("aaaeff54-400e-4edf-b116-def34ddb6a52"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Pak N Save Sylvia Park   Mt Wellingtonnz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("025a1c6f-b24d-41eb-8bcc-5f82ebd99daa"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 5, 3),
                    Description = "Pak N Save Sylvia Park Mt Wellington Nz",
                    LastMatch = new DateTime(2014, 5, 8),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("0282894f-4bca-4747-977e-35d542f8d7fb"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Paknsave Fuel Glen Innes Glen Innes   Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("72a4032d-1539-482e-93ff-1f3c15030370"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2013, 11, 9),
                    Description = "Paknsave Glen Innes      Glen Innes   Nz",
                    LastMatch = new DateTime(2013, 11, 9),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8c9faef5-61ef-4370-af22-015dbe5fe606"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 1, 2),
                    Description = "Pakuranga Lotto",
                    LastMatch = new DateTime(2014, 1, 2),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("9de7013d-6952-4de9-9384-d187d99f8b33"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 12),
                    Description = "Paper Plus Meadowbank    Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("a4fe0b97-a91c-4b92-9d30-f1a966ad9739"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2013, 11, 16),
                    Description = "Parents Inc              Greenlane    Nz",
                    LastMatch = new DateTime(2013, 11, 16),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("7a6df332-8dfe-49a3-963a-a5c6b816c480"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 3, 11),
                    Description = "Portmans #707 Sp         Mt Wellingtonnz",
                    LastMatch = new DateTime(2014, 3, 11),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("c1ebe964-6930-4d3d-982c-d8aa1230765a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 5, 8),
                    Description = "Portmans #707 Sp       Mt Wellington Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("cdd89ac3-07e4-4ded-b61f-710a467151b7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "INSCAR",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Protecta Insurance N",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e836dd3a-e648-4305-a7a6-432badd921d3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "ISALARY",
                    Created = new DateTime(2014, 4, 20),
                    Description = "Pulse It Ltd",
                    LastMatch = new DateTime(2014, 5, 18),
                    MatchCount = 8,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("1084ff14-6b68-4cab-9850-71f6f8acd304"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "ISALARY",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Pulse It Ltd      Dl",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4cac37b2-3a8b-4349-a0b6-e5f566ee258b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Pumpkin Patch - Auck     Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4d3c2cd0-6f95-488e-a3e4-9f82dcf06e19"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Quay Street Cafe         Auckland Citynz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("51eab070-c383-4701-a512-95bc19cc3f1b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "R K Convenience Store    Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("96125170-ec35-4792-9dbe-9bc2f4ac14b1"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 2, 21),
                    Description = "Remuera Village Med",
                    LastMatch = new DateTime(2014, 2, 21),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("02b91d02-f0cd-44f1-887a-4e07cc77a2ef"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 12),
                    Description = "Remuera Village Medical  Remuera Aucklnz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("d428c168-6ce7-4d5e-9284-709925cbcec9"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.HairBucketCode,
                    Created = new DateTime(2014, 4, 23),
                    Description = "Rodney Wayne Pakuranga   Pakuranga    Nz",
                    LastMatch = new DateTime(2014, 4, 23),
                    MatchCount = 12,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("109b17b6-8afd-42a8-b47b-8d17243aea29"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.HairBucketCode,
                    Created = new DateTime(2014, 4, 20),
                    Description = "Rodney Wayne Pakuranga Pakuranga     Nzl",
                    LastMatch = new DateTime(2014, 5, 18),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("965b30be-4d11-4823-905a-81b415a2418f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.HairBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Rodney Wayne Shampoo     Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("094a3b0f-089a-4fdc-a6e0-96fbc836c1c7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Ruan Thai                Mission Bay Anz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("d6f860e1-f05a-45ef-9b7d-8dda41156c2a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 3, 8),
                    Description = "Saint Johns Butchery     Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 8),
                    MatchCount = 5,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bf71220e-53c3-43a0-8521-36983addd630"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Sierra Cafe              Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("47935b66-aa75-43c5-9376-8e4233ac910d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 6),
                    Description = "Sierra Cafe Pakuranga    Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ccef69b1-b692-439b-be22-fcf06ad9ce0e"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 3, 8),
                    Description = "St Johns Dairy",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 5,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e1f49491-da25-4740-b102-9b5521ba3088"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 2, 21),
                    Description = "St Johns Dairy           Meadowbank Aunz",
                    LastMatch = new DateTime(2014, 2, 21),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("927e8893-a77c-4abd-b7e8-92e4755a733d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "INSCAR",
                    Created = new DateTime(2014, 3, 8),
                    Description = "State Insurance",
                    LastMatch = new DateTime(2014, 3, 8),
                    MatchCount = 1,
                    Reference1 = "Mot349886212",
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e6f93b58-6cae-4a5e-8c8b-e4eb98f7f40a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 1, 21),
                    Description = "Subway Beach Road        Auckland     Nz",
                    LastMatch = new DateTime(2014, 1, 21),
                    MatchCount = 4,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ccbe888e-15f0-4cf4-bf37-c3d4c4dfd262"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Subway Britomart         Auckland Citynz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("e9b3311c-3574-460e-8f71-89a344809e4c"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 6),
                    Description = "Subway Fort Street       Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 13,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8213ba24-4e5e-41c2-bdb1-c65aa3d58e98"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 28),
                    Description = "Subway Fort Street     Auckland      Nz",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 9,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4e35b807-d37c-4364-a47e-7b512154f851"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Subway Quay St           Parnell      Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("3d028063-0bce-424e-ba8d-7ddfe5c85659"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "Sunhill Fruit Centre",
                    LastMatch = new DateTime(2014, 5, 8),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("c783d70d-09bc-4313-b8fd-c2ed0cbf1260"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Sushi Sora",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("bb955567-ced9-4770-b8fc-5f6c89fc9657"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Sylvia Park Post Sho     Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8d6c962c-4cfe-421d-93be-247e5a509a97"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "CLOTHES",
                    Created = new DateTime(2014, 3, 11),
                    Description = "Temt Sylvia Park (7207)  Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 11),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ddc50d1a-4337-4420-8ecd-c5d52659dc37"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2013, 11, 9),
                    Description = "The Attic                Auckland     Nz",
                    LastMatch = new DateTime(2013, 11, 9),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("995fb874-455f-4f16-8267-271b97f7146c"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Coffee Club Takapu   Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("28acce42-79d4-49cf-832e-609b0aac2e2a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 12),
                    Description = "The Crown                Auckland     Nz",
                    LastMatch = new DateTime(2014, 4, 12),
                    MatchCount = 3,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("dc16bcdb-c231-4d13-9124-755014bacd3a"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 28),
                    Description = "The Lunch Factory",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 6,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("d4a9de9d-d4ac-4b25-9d87-2ed94c0add77"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Mad Butcher          Glen Innes   Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("25aecdd8-0e7e-4dca-9856-9740bb804c1d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Paddington           Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("35034d1d-bef0-4d74-8fff-57be724ed2cc"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Parenting Place      Greenlane    Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("52658578-863e-4698-8d34-b49a1dba7494"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Pita Pit Takapuna    Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ed07bc28-3414-4093-aeb3-6b2d9cd90428"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Store On Kohi Ltd    Kohimarama   Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("881a3e45-ec12-4d4d-8b7e-c8ab52637096"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Video Shop           Meadowbank Aknz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4db0360a-f19a-4fcd-8025-04d9fcdf0e7e"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 6),
                    Description = "The Video Shop           Saint Johns Anz",
                    LastMatch = new DateTime(2014, 4, 6),
                    MatchCount = 10,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("70c7be3d-40d9-4392-81fd-ff06d6a4eafb"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "The Video Shop         Saint Johns A Nz",
                    LastMatch = new DateTime(2014, 6, 6),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("9d73ecdd-3410-4177-86e2-b6d4c1da2147"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Toyworld Manukau         Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("6de88270-251f-407e-81a2-55decbeb752d"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Trade Me Limited         Wellington   Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("7a89a15c-17e9-45e1-9499-65671b9eb85b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Tyler St Garage          Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("3e5602dd-c156-4051-a681-d0a4c649b8b7"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Unihealth Pharmacy       Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("aeca8a53-72ac-4783-b8e3-2fb5b027a8ee"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.PhoneBucketCode,
                    Created = new DateTime(2014, 4, 28),
                    Description = "Vodafone",
                    LastMatch = new DateTime(2014, 5, 30),
                    MatchCount = 10,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("8d745f02-4c4a-4bea-85b2-958fe723ced8"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.FoodBucketCode,
                    Created = new DateTime(2014, 3, 8),
                    Description = "V'S Fruit & Vege         Saint Johns  Nz",
                    LastMatch = new DateTime(2014, 3, 8),
                    MatchCount = 6,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("33c7fcdd-467d-4e33-a1cb-d117cd4fc339"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Wagamama Sylvia Park     Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("a48dcedf-f67f-4a21-8b7a-16b1db2301a6"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = TestDataConstants.WaterBucketCode,
                    Created = new DateTime(2014, 4, 20),
                    Description = "Watercare Services",
                    LastMatch = new DateTime(2014, 5, 18),
                    MatchCount = 8,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("62a5fd87-1cc9-4faf-a13f-0cb25440ef2f"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Wendys Old Fashioned Ham Panmure      Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("ac6871f8-2caa-4765-882f-b04ebd2b413b"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "DOC",
                    Created = new DateTime(2014, 4, 29),
                    Description = "White Cross Healthca     Auckland     Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("b121da15-931a-4c9f-b2df-98e7f5b7fbba"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "SURPLUS",
                    Created = new DateTime(2014, 3, 8),
                    Description = "Wilson Parking           Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 8),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("68603075-5152-46be-abb4-04b6f8e05411"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 10, 19),
                    Description = "Z Glen Innes             Auckland     Nz",
                    LastMatch = new DateTime(2013, 10, 19),
                    MatchCount = 2,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("02db78ce-9937-400a-aead-22b4281721f3"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 12, 2),
                    Description = "Z Kepa Road              Orakei       Nz",
                    LastMatch = new DateTime(2013, 12, 2),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("9909156c-c610-4500-8131-9d8e50f88714"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 5, 3),
                    Description = "Z Kepa Road            Auckland      Nz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("b164af05-e62c-4c4e-b824-2866acb85abe"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 4, 29),
                    Description = "Z Panmure                Mt Wellingtonnz",
                    LastMatch = null,
                    MatchCount = 0,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("c6991443-3dfe-490f-a7f5-620b23204208"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2013, 12, 7),
                    Description = "Z Panmure                Panmure      Nz",
                    LastMatch = new DateTime(2013, 12, 7),
                    MatchCount = 1,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("4922d9fd-b9d5-4433-9ad0-b60db232b089"),
                    TransactionType = null,
                },
                new MatchingRule(BucketRepo)

                {
                    Amount = null,
                    BucketCode = "FUEL",
                    Created = new DateTime(2014, 3, 28),
                    Description = "Z Quay Street            Auckland     Nz",
                    LastMatch = new DateTime(2014, 3, 28),
                    MatchCount = 11,
                    Reference1 = null,
                    Reference2 = null,
                    Reference3 = null,
                    RuleId = new Guid("29ccd6d7-389b-4f27-8901-1ecf7ed237c8"),
                    TransactionType = null,
                },
            };
        }
    }
}