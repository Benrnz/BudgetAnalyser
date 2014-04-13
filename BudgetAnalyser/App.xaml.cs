using System;
using System.Diagnostics;
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
    public partial class App : Application
    {
        private ILogger logger;

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
            compositionRoot.RegisterIoCMappings();
            this.logger = compositionRoot.Logger;
            this.logger.LogAlways(() => "=========== Budget Analyser Starting ===========");
            this.logger.LogAlways(() => compositionRoot.ShellController.DashboardController.VersionString);
            var initialisableShell = compositionRoot.ShellController as IInitializableController;
            if (initialisableShell != null)
            {
                initialisableShell.Initialize();
            }

            compositionRoot.ShellWindow.DataContext = compositionRoot.ShellController;
            this.logger.LogInfo(() => "Initialisation finished.");
            compositionRoot.ShellWindow.Show();
        }

        private void LogUnhandledException(string origin, object ex)
        {
            var builder = new StringBuilder();
            builder.AppendLine(string.Empty);
            builder.AppendLine("=====================================================================================");
            builder.AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture));
            builder.AppendLine("Unhandled exception was thrown from orgin: " + origin);
            builder.AppendLine(ex.ToString());
            this.logger.LogError(builder.ToString);

            Current.Shutdown();
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            Current.Exit -= OnApplicationExit;
            Messenger.Default.Send(new ShutdownMessage());
            this.logger.LogAlways(() => "Application Exiting");
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