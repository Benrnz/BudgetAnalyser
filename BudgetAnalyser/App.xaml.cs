using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using Rees.Wpf;

namespace BudgetAnalyser
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
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
            var initialisableShell = compositionRoot.ShellController as IInitializableController;
            if (initialisableShell != null)
            {
                initialisableShell.Initialize();
            }

            compositionRoot.ShellWindow.DataContext = compositionRoot.ShellController;
            compositionRoot.ShellWindow.Show();
        }

        private static void LogUnhandledException(string origin, object ex)
        {
            //Logger.Instance.WriteEntry(string.Empty);
            //Logger.Instance.WriteEntry("=====================================================================================");
            //Logger.Instance.WriteEntry(DateTime.Now);
            //Logger.Instance.WriteEntry("Unhandled exception was thrown from orgin: " + origin);
            //string message = ex.ToString();
            //Logger.Instance.WriteEntry(message);

            //new WindowsMessageBox().Show(TypeVisualiser.Properties.Resources.Application_An_Unhandled_Exception_Occurred, (object)message);
            Debug.WriteLine(ex.ToString());
            Current.Shutdown();
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            Current.Exit -= OnApplicationExit;
            Messenger.Default.Send(new ShutdownMessage());
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