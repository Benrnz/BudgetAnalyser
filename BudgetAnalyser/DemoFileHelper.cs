using System.Diagnostics;
using System.IO;
using System.Reflection;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class DemoFileHelper
    {
        private const string DemoFileName = "BudgetAnalyserDemo.bax";

        public string FindDemoFile()
        {
            var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Debug.Assert(folder is not null);
            for (var failsafe = 0; failsafe < 10; failsafe++)
            {
                var path = Path.Combine(folder, DemoFileName);
                if (FileExists(path))
                {
                    return path;
                }

                path = Path.Combine(folder, "TestData", DemoFileName);
                if (FileExists(path))
                {
                    return path;
                }

                folder = Directory.GetParent(folder).FullName;
            }

            throw new FileNotFoundException();
        }

        protected virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }
    }
}
