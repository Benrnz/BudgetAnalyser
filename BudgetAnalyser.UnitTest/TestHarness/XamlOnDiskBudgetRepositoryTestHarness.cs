using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Budget.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskBudgetRepositoryTestHarness : XamlOnDiskBudgetRepository
    {
        public XamlOnDiskBudgetRepositoryTestHarness([NotNull] IBudgetBucketRepository bucketRepository)
            : base(bucketRepository,
                new BudgetCollectionToBudgetCollectionDtoMapper(new BudgetModelToBudgetModelDtoMapper()),
                new BudgetCollectionDtoToBudgetCollectionMapper(new BudgetModelDtoToBudgetModelMapper()))
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

        protected override object LoadFromDisk(string fileName)
        {
            if (LoadFromDiskMock == null)
            {
                return base.LoadFromDisk(fileName);
            }
            return LoadFromDiskMock(fileName);
        }

        protected override void WriteToDisk(string fileName, string data)
        {
            if (WriteToDiskMock == null)
            {
                base.WriteToDisk(fileName, data);
            }
            else
            {
                WriteToDiskMock(fileName, data);
            }
        }
    }
}