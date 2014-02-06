using System.IO;
using System.Reflection;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class DemoFileHelper
    {
        public string FindDemoFile(string demoFilename)
        {
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            for (int failsafe = 0; failsafe < 10; failsafe++)
            {
                string path = Path.Combine(folder, demoFilename);
                if (FileExists(path))
                {
                    return path;
                }

                path = Path.Combine(folder, "TestData", demoFilename);
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