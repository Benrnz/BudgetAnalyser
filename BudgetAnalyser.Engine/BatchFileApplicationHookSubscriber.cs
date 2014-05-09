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
    [AutoRegisterWithIoC]
    public sealed class BatchFileApplicationHookSubscriber : IApplicationHookSubscriber, IDisposable
    {
        private const string BatchFileName = "BudgetAnalyserHooks.bat";
        private readonly ILogger logger;
        private readonly IEnumerable<IApplicationHookEventPublisher> publishers;
        private string doNotUseFileName;
        private bool isDisposed;

        public BatchFileApplicationHookSubscriber([NotNull] IEnumerable<IApplicationHookEventPublisher> publishers, [NotNull] ILogger logger)
        {
            if (publishers == null)
            {
                throw new ArgumentNullException("publishers");
            }

            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.publishers = publishers.ToList();
            this.logger = logger;

            foreach (IApplicationHookEventPublisher publisher in this.publishers)
            {
                publisher.ApplicationEvent += OnEventOccurred;
            }
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

        public void Dispose()
        {
            this.isDisposed = true;
            this.logger.LogInfo(() => "BatchFileApplicationHookSubscriber is being disposed.");
            foreach (IApplicationHookEventPublisher publisher in this.publishers)
            {
                publisher.ApplicationEvent -= OnEventOccurred;
            }

            GC.SuppressFinalize(this);
        }

        public void OnEventOccurred(object sender, ApplicationHookEventArgs args)
        {
            if (this.isDisposed)
            {
                return;
            }

            Task.Factory.StartNew(
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
    }
}