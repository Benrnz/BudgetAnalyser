using System;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskBudgetRepositoryTestHarness : XamlOnDiskBudgetRepository
    {
        public XamlOnDiskBudgetRepositoryTestHarness([NotNull] IBudgetBucketRepository bucketRepository)
            : base(bucketRepository, new BudgetCollectionToDtoMapper(), new DtoToBudgetCollectionMapper())
        {
        }

        public XamlOnDiskBudgetRepositoryTestHarness(
            IBudgetBucketRepository bucketRepo, 
            BasicMapper<BudgetCollection, BudgetCollectionDto> toDtoMapper, 
            BasicMapper<BudgetCollectionDto, BudgetCollection> toDomainMapper)
            : base(bucketRepo, toDtoMapper, toDomainMapper) { }

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

        protected async override Task<object> LoadFromDisk(string fileName)
        {
            if (LoadFromDiskMock == null)
            {
                return await base.LoadFromDisk(fileName);
            }

            return LoadFromDiskMock(fileName);
        }

        protected async override Task WriteToDisk(string fileName, string data)
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