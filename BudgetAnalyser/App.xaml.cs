using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BudgetAnalyser.Engine;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IApplicationHookEventPublisher
    {
        private ILogger logger;
        public event EventHandler<ApplicationHookEventArgs> ApplicationEvent;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            Current.Exit += OnApplicationExit;

            var compositionRoot = new CompositionRoot();
            compositionRoot.Compose(this);
            this.logger = compositionRoot.Logger;

            this.logger.LogAlways(_ => "=========== Budget Analyser Starting ===========");
            this.logger.LogAlways(_ => compositionRoot.ShellController.DashboardController.VersionString);
            var initialisableShell = compositionRoot.ShellController as IInitializableController;
            if (initialisableShell != null)
            {
                initialisableShell.Initialize();
            }

            compositionRoot.ShellWindow.DataContext = compositionRoot.ShellController;
            this.logger.LogInfo(_ => "Initialisation finished.");
            compositionRoot.ShellWindow.Show();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "All exceptions are already logged, any further exceptions attempting to gracefully shutdown can be ignored.")]
        private void LogUnhandledException(string origin, object ex)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Empty);
            builder.AppendLine("=====================================================================================");
            builder.AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture));
            builder.AppendLine("Unhandled exception was thrown from orgin: " + origin);
            builder.AppendLine(ex.ToString());
            this.logger.LogError(_ => builder.ToString());

            // If you get a NullReference Exception with no inner exception here its most likely because a class takes an interface in its constructor that Autofac doesn't have a registration for.
            // Most likely you forgot to annotate an implementation of an interface with [AutoRegisterWithIoc]
            try
            {
                Current.Shutdown();
            }
            catch
            {
                // Exceptions are already handled and logged, ignore all others attempting to shut down.
            }
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            var handler = ApplicationEvent;
            if (handler != null)
            {
                handler(this, new ApplicationHookEventArgs(ApplicationHookEventType.Application, "App.xaml.cs", ApplicationHookEventArgs.Exit));
            }

            Current.Exit -= OnApplicationExit;
            Messenger.Default.Send(new ShutdownMessage());
            this.logger.LogAlways(_ => "=========== Application Exiting ===========");
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogUnhandledException("App.OnCurrentDomainUnhandledException", e.ExceptionObject);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogUnhandledException("App.OnDispatcherUnhandledException", e.Exception);
        }
    }
}