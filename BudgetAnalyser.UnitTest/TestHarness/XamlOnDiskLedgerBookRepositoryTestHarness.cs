using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskLedgerBookRepositoryTestHarness : XamlOnDiskLedgerBookRepository
    {
        public XamlOnDiskLedgerBookRepositoryTestHarness(
            [NotNull] ILedgerDataToDomainMapper dataToDomainMapper,
            [NotNull] ILedgerDomainToDataMapper domainToDataMapper
            ) : base(dataToDomainMapper, domainToDataMapper)
        {
        }

        public Func<string, bool> FileExistsMock { get; set; }

        public Action<DataLedgerBook> SaveXamlFileToDiskMock { get; set; }

        protected override bool FileExistsOnDisk(string fileName)
        {
            return FileExistsMock(fileName);
        }

        protected override DataLedgerBook LoadXamlFromDisk(string fileName)
        {
            // Using an old Embedded Resource
            // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
            Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException("Cannot find resource named: " + fileName);
                }

                return (DataLedgerBook)XamlServices.Load(new XamlXmlReader(stream));
            }
        }

        protected override void SaveXamlFileToDisk(DataLedgerBook dataEntity)
        {
            SaveXamlFileToDiskMock(dataEntity);
        }
    }
}