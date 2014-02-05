using System.IO;
using System.Reflection;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class DemoFileHelper
    {
        protected virtual bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string FindDemoFile(string ledgerBookDemoFile)
        {
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            for (int failsafe = 0; failsafe < 10; failsafe++)
            {
                string path = Path.Combine(folder, ledgerBookDemoFile);
                if (FileExists(path))
                {
                    return path;
                }

                path = Path.Combine(folder, "TestData", ledgerBookDemoFile);
                if (FileExists(path))
                {
                    return path;
                }

                folder = Directory.GetParent(folder).FullName;
            }

            throw new FileNotFoundException();
        }
    }
}
