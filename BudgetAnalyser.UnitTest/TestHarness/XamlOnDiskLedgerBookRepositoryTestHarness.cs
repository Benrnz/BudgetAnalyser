using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Xaml;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Ledger.Data;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class XamlOnDiskLedgerBookRepositoryTestHarness : XamlOnDiskLedgerBookRepository
    {
        public XamlOnDiskLedgerBookRepositoryTestHarness(
            [NotNull] ILedgerDataToDomainMapper dataToDomainMapper,
            [NotNull] ILedgerDomainToDataMapper domainToDataMapper
            ) : base(dataToDomainMapper, domainToDataMapper, new FakeLogger())
        {
        }

        public Func<string, bool> FileExistsMock { get; set; }

        public Action<LedgerBookDto> SaveXamlFileToDiskMock { get; set; }

        protected override bool FileExistsOnDisk(string fileName)
        {
            return FileExistsMock(fileName);
        }

        protected override LedgerBookDto LoadXamlFromDisk(string fileName)
        {
            // this line of code is useful to figure out the name Vs has given the resource! The name is case sensitive.
            Assembly.GetExecutingAssembly().GetManifestResourceNames().ToList().ForEach(n => Debug.WriteLine(n));
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                if (stream == null)
                {
                    throw new MissingManifestResourceException("Cannot find resource named: " + fileName);
                }

                return (LedgerBookDto)XamlServices.Load(new XamlXmlReader(stream));
            }
        }

        protected override void SaveXamlFileToDisk(LedgerBookDto dataEntity)
        {
            SaveXamlFileToDiskMock(dataEntity);
        }
    }
}