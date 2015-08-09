using System;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Statement;
using BudgetAnalyser.Engine.Statement.Data;
using BudgetAnalyser.UnitTest.TestHarness;

namespace BudgetAnalyser.UnitTest.TestData
{
    public static class StatementModelTestDataGenerator
    {
        public static void GenerateCSharp(string fileName, DateTime beginDate, DateTime endDate)
        {
            var fakeLogger = new FakeLogger();
            var importer = new CsvOnDiskStatementModelRepositoryV1(
                new BankImportUtilities(fakeLogger),
                fakeLogger,
                new TransactionSetDtoToStatementModelMapper(),
                new StatementModelToTransactionSetDtoMapper());

            Task<StatementModel> modelTask = importer.LoadAsync(fileName);
            modelTask.Wait();
            StatementModel model = modelTask.Result;

            Console.WriteLine(@"
/// <summary>THIS IS GENERATED CODE </summary>
[GeneratedCode(""StatementModelTestDataGenerator.GenerateCSharp"", ""{0}"")]
public static StatementModel TestDataGenerated()
{{
    var model = new StatementModel(new FakeLogger()) 
    {{", DateTime.Now);
            Console.WriteLine(@"
        StorageKey = @""C:\Foo\StatementModel.csv"",");
            Console.WriteLine(@"
        Imported = new DateTime({0}, {1}, {2}),", model.LastImport.Year, model.LastImport.Month, model.LastImport.Day);
            Console.WriteLine(@"
    };"); // End new StatementModel Initialiser

            Console.WriteLine(@"
    var transactions = new List<Transaction>
    {");
            foreach (Transaction transaction in model.AllTransactions.Where(t => t.Date >= beginDate && t.Date <= endDate))
            {
                Console.WriteLine(@"
        new Transaction 
        {");
                Console.WriteLine(@"
            Account = AccountTypeRepo.GetByKey(""{0}""),", transaction.Account.Name);
                Console.WriteLine(@"
            Amount = {0}M,", transaction.Amount);
                Console.WriteLine(@"
            BudgetBucket = BudgetBucketRepo.GetByCode(""{0}""),", transaction.BudgetBucket.Code);
                Console.WriteLine(@"
            Date = new DateTime({0}, {1}, {2}),", transaction.Date.Year, transaction.Date.Month, transaction.Date.Day);
                Console.WriteLine(@"
            Description = ""{0}"",", transaction.Description);
                Console.WriteLine(@"
            Id = new Guid(""{0}""),", transaction.Id);
                Console.WriteLine(@"
            Reference1 = ""{0}"",", transaction.Reference1);
                Console.WriteLine(@"
            Reference2 = ""{0}"",", transaction.Reference2);
                Console.WriteLine(@"
            Reference3 = ""{0}"",", transaction.Reference3);
                Console.WriteLine(@"
            TransactionType = new NamedTransaction(""{0}""),", transaction.TransactionType);

                Console.WriteLine(@"
        },");
            }

            Console.WriteLine(@"
    };"); // End transactions list initialiser

            Console.WriteLine(@"
    return model.LoadTransactions(transactions);");
            Console.WriteLine(@"
}"); // End Method
        }
    }
}