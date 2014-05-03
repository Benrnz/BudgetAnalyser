using System;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskBudgetRepositoryTestHarness : XamlOnDiskBudgetRepository
    {
        public XamlOnDiskBudgetRepositoryTestHarness([NotNull] IBudgetBucketRepository bucketRepository)
            : base(bucketRepository,
                new BudgetCollectionToDataBudgetCollectionMapper(new BudgetModelToDataBudgetModelMapper()),
                new DataBudgetCollectionToBudgetCollectionMapper(new DataBudgetModelToBudgetModelMapper()))
        {
        }

        public Func<string, bool> FileExistsMock { get; set; }

        public Func<string, object> LoadFromDiskMock { get; set; }

        public Action<string, string> WriteToDiskMock { get; set; }

        protected override bool FileExists(string filename)
        {
            if (FileExistsMock == null)
            {
                return base.FileExists(filename);
            }
            return FileExistsMock(filename);
        }

        protected override object LoadFromDisk(string filename)
        {
            if (LoadFromDiskMock == null)
            {
                return base.LoadFromDisk(filename);
            }
            return LoadFromDiskMock(filename);
        }

        protected override void WriteToDisk(string filename, string data)
        {
            if (WriteToDiskMock == null)
            {
                base.WriteToDisk(filename, data);
            }
            else
            {
                WriteToDiskMock(filename, data);
            }
        }
    }
}