using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
    public class BatchFileApplicationHookSubscriber : IApplicationHookSubscriber
    {
        private const string BatchFileName = "BudgetAnalyserHooks.bat";
        private readonly ILogger logger;
        private string doNotUseFileName;
        private IEnumerable<IApplicationHookEventPublisher> myPublishers;

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

        public void Subscribe([NotNull] IEnumerable<IApplicationHookEventPublisher> publishers)
        {
            if (publishers == null)
            {
                throw new ArgumentNullException("publishers");
            }

            this.myPublishers = publishers.ToList();
            foreach (IApplicationHookEventPublisher publisher in this.myPublishers)
            {
                publisher.ApplicationEvent += OnEventOccurred;
            }
        }

        protected virtual Task PerformAction(object sender, ApplicationHookEventArgs args)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    if (!File.Exists(FileName))
                    {
                        using (File.CreateText(FileName))
                        {
                        }
                    }

                    string commandLine = string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} \"{1}\" \"{2}\" \"{3}\" \"{4}\" \"{5}\"",
                        FileName,
                        args.EventType,
                        args.EventSubcategory,
                        args.Origin,
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        sender);
                    this.logger.LogInfo(() => "Executing batch file with commandline: " + commandLine);

                    var processInfo = new ProcessStartInfo("cmd.exe", "/c " + commandLine)
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = false,
                        RedirectStandardOutput = false,
                    };
                    Process process = Process.Start(processInfo);
                    process.WaitForExit(5000);
                    this.logger.LogInfo(() => "Output from commandline:\n" + process.StandardOutput.ReadToEnd());
                });
        }

        private void OnEventOccurred(object sender, ApplicationHookEventArgs args)
        {
            PerformAction(sender, args);
        }
    }
}