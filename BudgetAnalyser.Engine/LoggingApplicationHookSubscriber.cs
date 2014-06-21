using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    [AutoRegisterWithIoC]
    public sealed class LoggingApplicationHookSubscriber : IApplicationHookSubscriber, IDisposable
    {
        private readonly ILogger logger;
        private IEnumerable<IApplicationHookEventPublisher> myPublishers;
        private bool isDisposed;

        public LoggingApplicationHookSubscriber([NotNull] ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }

            this.logger = logger;
        }

        public void Dispose()
        {
            this.isDisposed = true;
            this.logger.LogInfo(() => "LoggingApplicationHookSubscriber is being disposed.");
            if (this.myPublishers != null)
            {
                foreach (IApplicationHookEventPublisher publisher in this.myPublishers)
                {
                    publisher.ApplicationEvent -= OnEventOccurred;
                }
            }

            GC.SuppressFinalize(this);
        }

        private void OnEventOccurred(object sender, ApplicationHookEventArgs args)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.logger.LogInfo(() => string.Format(CultureInfo.CurrentCulture, "Application Hook Event Occurred. Sender: [{0}], Type: {1}, Origin: {2}", sender, args.EventType, args.Origin));
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
    }
}