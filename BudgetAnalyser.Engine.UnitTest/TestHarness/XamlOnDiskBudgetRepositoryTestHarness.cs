using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;
using JetBrains.Annotations;

namespace BudgetAnalyser.Engine.UnitTest.TestHarness
{
    public class XamlOnDiskBudgetRepositoryTestHarness : XamlOnDiskBudgetRepository
    {
        public XamlOnDiskBudgetRepositoryTestHarness([NotNull] IBudgetBucketRepository bucketRepository)
            : base(
                  bucketRepository, 
                  new Mapper_BudgetCollectionDto_BudgetCollection(
                      bucketRepository, 
                      new Mapper_BudgetBucketDto_BudgetBucket(new BudgetBucketFactory()), 
                      new Mapper_BudgetModelDto_BudgetModel(bucketRepository)))
        {
        }

        public XamlOnDiskBudgetRepositoryTestHarness(
            IBudgetBucketRepository bucketRepo,
            IDtoMapper<BudgetCollectionDto, BudgetCollection> mapper)
            : base(bucketRepo, mapper)
        {
        }

        public Func<string, bool> FileExistsMock { get; set; }
        public Func<string, object> LoadFromDiskMock { get; set; }
        public Action<string, string> WriteToDiskMock { get; set; }

        protected override bool FileExists(string fileName)
        {
            if (FileExistsMock == null)
            {
                return base.FileExists(fileName);
            }
            return FileExistsMock(fileName);
        }

        protected override async Task<object> LoadFromDisk(string fileName)
        {
            if (LoadFromDiskMock == null)
            {
                return await base.LoadFromDisk(fileName);
            }

            return LoadFromDiskMock(fileName);
        }

        protected override async Task WriteToDisk(string fileName, string data)
        {
            if (WriteToDiskMock == null)
            {
                await base.WriteToDisk(fileName, data);
            }
            else
            {
                WriteToDiskMock(fileName, data);
            }
        }
    }
}