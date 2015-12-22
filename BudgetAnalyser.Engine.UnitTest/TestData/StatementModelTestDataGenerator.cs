//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;
//using BudgetAnalyser.Engine.Statement;
//using BudgetAnalyser.Engine.Statement.Data;
//using BudgetAnalyser.UnitTest.TestHarness;

//namespace BudgetAnalyser.UnitTest.TestData
//{
//    /// <summary>
//    /// Use this class to extract statement testdata from a csv file and then generate the corresponding StatementModel in CSharp.
//    /// </summary>
//    public static class StatementModelTestDataGenerator
//    {
//        public static void GenerateCSharp(string fileName, DateTime beginDate, DateTime endDate)
//        {
//            var fakeLogger = new FakeLogger();
//            var importer = new CsvOnDiskStatementModelRepositoryV1(
//                new BankImportUtilities(fakeLogger),
//                fakeLogger,
//                new TransactionSetDtoToStatementModelMapper(),
//                new StatementModelToTransactionSetDtoMapper());

//            Task<StatementModel> modelTask = importer.LoadAsync(fileName);
//            modelTask.Wait();
//            StatementModel model = modelTask.Result;

//            Debug.WriteLine(@"
///// <summary>THIS IS GENERATED CODE </summary>
//[GeneratedCode(""StatementModelTestDataGenerator.GenerateCSharp"", ""{0}"")]
//public static StatementModel TestDataGenerated()
//{{
//    var model = new StatementModel(new FakeLogger()) 
//    {{", DateTime.Now);
//            Debug.WriteLine(@"
//        StorageKey = @""C:\Foo\StatementModel.csv"",");
//            Debug.WriteLine(@"
//        Imported = new DateTime({0}, {1}, {2}),", model.LastImport.Year, model.LastImport.Month, model.LastImport.Day);
//            Debug.WriteLine(@"
//    };"); // End new StatementModel Initialiser

//            Debug.WriteLine(@"
//    var transactions = new List<Transaction>
//    {");
//            foreach (Transaction transaction in model.AllTransactions.Where(t => t.Date >= beginDate && t.Date <= endDate))
//            {
//                Debug.WriteLine(@"
//        new Transaction 
//        {");
//                Debug.WriteLine(@"
//            Account = AccountTypeRepo.GetByKey(""{0}""),", transaction.Account.Name);
//                Debug.WriteLine(@"
//            Amount = {0}M,", transaction.Amount);
//                Debug.WriteLine(@"
//            BudgetBucket = BudgetBucketRepo.GetByCode(""{0}""),", transaction.BudgetBucket.Code);
//                Debug.WriteLine(@"
//            Date = new DateTime({0}, {1}, {2}),", transaction.Date.Year, transaction.Date.Month, transaction.Date.Day);
//                Debug.WriteLine(@"
//            Description = ""{0}"",", transaction.Description);
//                Debug.WriteLine(@"
//            Id = new Guid(""{0}""),", transaction.Id);
//                Debug.WriteLine(@"
//            Reference1 = ""{0}"",", transaction.Reference1);
//                Debug.WriteLine(@"
//            Reference2 = ""{0}"",", transaction.Reference2);
//                Debug.WriteLine(@"
//            Reference3 = ""{0}"",", transaction.Reference3);
//                Debug.WriteLine(@"
//            TransactionType = new NamedTransaction(""{0}""),", transaction.TransactionType);

//                Debug.WriteLine(@"
//        },");
//            }

//            Debug.WriteLine(@"
//    };"); // End transactions list initialiser

//            Debug.WriteLine(@"
//    return model.LoadTransactions(transactions);");
//            Debug.WriteLine(@"
//}"); // End Method
//        }
//    }
//}