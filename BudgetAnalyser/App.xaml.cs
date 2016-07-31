using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This is the root object in the App")]
    public partial class App
    {
        private CompositionRoot compositionRoot;
        private ILogger logger;
        private ShellController shellController;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Ensure the current culture passed into bindings is the OS culture.
            // By default, WPF uses en-US as the culture, regardless of the system settings.
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            Current.Exit += OnApplicationExit;

            this.compositionRoot = new CompositionRoot();
            this.compositionRoot.Compose();
            this.logger = this.compositionRoot.Logger;

            this.logger.LogAlways(_ => "=========== Budget Analyser Starting ===========");
            this.logger.LogAlways(_ => this.compositionRoot.ShellController.DashboardController.VersionString);
            this.shellController = this.compositionRoot.ShellController;
            this.shellController?.Initialize();

            this.compositionRoot.ShellWindow.DataContext = this.compositionRoot.ShellController;
            this.logger.LogInfo(_ => "Initialisation finished.");
            this.compositionRoot.ShellWindow.Show();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "All exceptions are already logged, any further exceptions attempting to gracefully shutdown can be ignored.")]
        private void LogUnhandledException(string origin, object ex)
        {
            if (this.logger != null)
            {
                var builder = new StringBuilder();
                builder.AppendLine(string.Empty);
                builder.AppendLine("=====================================================================================");
                builder.AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture));
                builder.AppendLine("Unhandled exception was thrown from orgin: " + origin);
                builder.AppendLine(ex.ToString());
                this.logger.LogError(_ => builder.ToString());
            }

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
            Current.Exit -= OnApplicationExit;
            this.compositionRoot.Dispose();
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