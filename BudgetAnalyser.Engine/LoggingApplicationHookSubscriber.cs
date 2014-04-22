using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    [AutoRegisterWithIoC]
    public class LoggingApplicationHookSubscriber : IApplicationHookSubscriber, IDisposable
    {
        private readonly ILogger logger;
        private readonly IEnumerable<IApplicationHookEventPublisher> publishers;
        private bool isDisposed;

        public LoggingApplicationHookSubscriber([NotNull] IEnumerable<IApplicationHookEventPublisher> publishers, [NotNull] ILogger logger)
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
                publisher.ApplicationEvent += OnEventOccured;
            }
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.logger.LogInfo(() => "LoggingApplicationHookSubscriber is being disposed.");
            foreach (IApplicationHookEventPublisher publisher in this.publishers)
            {
                publisher.ApplicationEvent -= OnEventOccured;
            }
        }

        public void OnEventOccured(object sender, ApplicationHookEventArgs args)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.logger.LogInfo(() => string.Format(CultureInfo.CurrentCulture, "Application Hook Event Occurred. Sender: [{0}], Type: {1}, Origin: {2}", sender, args.EventType, args.Origin));
        }
    }
}