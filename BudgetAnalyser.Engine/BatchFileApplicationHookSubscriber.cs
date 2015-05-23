using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    /// <summary>
    ///     An application event subscriber class that will run a batch file whenever the publishers raise an event.
    ///     This allows for automatic source control of data files. For example, it can check-in into subversion when ever the
    ///     statement file is changed.
    /// </summary>
    [AutoRegisterWithIoC]
    [UsedImplicitly]
    public class BatchFileApplicationHookSubscriber
    {
        private const string BatchFileName = "BudgetAnalyserHooks.bat";
        private readonly ILogger logger;
        private string doNotUseFileName;

        public BatchFileApplicationHookSubscriber([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        private string FileName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.doNotUseFileName))
                {
                    string path = Path.GetDirectoryName(GetType().Assembly.Location);
                    this.doNotUseFileName = Path.Combine(path, BatchFileName);
                }

                return this.doNotUseFileName;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "reviewed ok here.")]
        public void PerformAction()
        {
            if (!File.Exists(FileName))
            {
                using (File.CreateText(FileName))
                {
                }
            }

            string commandLine = string.Format(
                CultureInfo.CurrentCulture,
                "{0} \"{1}\" \"{2}\" \"{3}\" \"{4}\"",
                FileName,
                "Save",
                "Exit",
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                GetType().Name);
            this.logger.LogInfo(_ => "Executing batch file with commandline: " + commandLine);

            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + commandLine)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                RedirectStandardError = false,
                RedirectStandardOutput = false,
            };
            try
            {
                Process process = Process.Start(processInfo);
                process.WaitForExit(15000);
                this.logger.LogInfo(_ => "Output from commandline:\n" + process.StandardOutput.ReadToEnd());
            }
            catch
            {
                // Ignore - Best efforts to log only and app is exiting.
            }
        }
    }
}