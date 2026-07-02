using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BudgetAnalyser;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This is the root object in the App")]
public partial class App
{
    private IHost host = null!;
    private ILogger logger = null!;
    private ShellController? shellController;
    private Stopwatch stopwatch = Stopwatch.StartNew();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Ensure the current culture passed into bindings is the OS culture.
        // By default, WPF uses en-US as the culture, regardless of the system settings.
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

        Current.Exit += OnApplicationExit;

        var encryptionAssembly = typeof(IFileEncryptor).GetTypeInfo().Assembly;
        var thisAssembly = typeof(CompositionHelper).GetTypeInfo().Assembly;

        this.host = Host.CreateDefaultBuilder()
            .UseDefaultServiceProvider((_, options) =>
            {
#if DEBUG
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
#endif
            })
            .ConfigureServices((_, services) =>
            {
                services.AddEngineRegistrations();
                services.AddAutoRegistrations(encryptionAssembly);
                services.AddAutoRegistrations(thisAssembly);
                services.AddReesWpfRegistrations();
                services.AddBudgetAnalyserRegistrations();
            })
            .Build();

        this.host.StartAsync().GetAwaiter().GetResult();

        this.stopwatch.Stop();
        var registrationTime = this.stopwatch.ElapsedMilliseconds;
        this.stopwatch = Stopwatch.StartNew();

        this.logger = this.host.Services.GetRequiredService<ILogger>();
        this.logger.LogLevelFilter = LogLevel.Info; // hardcoded default log level.
        this.logger.LogAlways(_ => "=========== Budget Analyser is Starting ===========");

        CompositionHelper.BuildApplicationObjectGraph(this.host.Services);
        this.shellController = this.host.Services.GetRequiredService<ShellController>();
        this.logger.LogAlways(_ => this.shellController.TopDashboardController.VersionString);

        CompositionHelper.LoadApplicationStateIntoControllers(
            this.logger,
            this.host.Services.GetRequiredService<IPersistApplicationState>(),
            this.host.Services.GetRequiredService<IMessenger>());

        var topLevelWindow = new ShellWindow { DataContext = this.shellController };
        this.logger.LogInfo(_ => "Initialisation finished.");
        topLevelWindow.Show();

        this.stopwatch.Stop();
        var initialisationTime = this.stopwatch.ElapsedMilliseconds;
        this.logger.LogAlways(_ => $"Registration took {registrationTime:N0}ms. Initialisation took {initialisationTime:N0}ms.");
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
        Justification = "All exceptions are already logged, any further exceptions attempting to gracefully shutdown can be ignored.")]
    private void LogUnhandledException(string origin, object ex)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Empty);
        builder.AppendLine("=====================================================================================");
        builder.AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture));
        builder.AppendLine("Unhandled exception was thrown from orgin: " + origin);
        builder.AppendLine(ex.ToString());
        this.logger?.LogError(_ => builder.ToString());

        // If you get a NullReference Exception with no inner exception here its most likely because a class takes an interface in its constructor that the DI container doesn't have a registration for.
        // Most likely you forgot to annotate an implementation of an interface with [AutoRegisterWithIoC]
        try
        {
            Current.Shutdown();
        }
        catch
        {
            // Exceptions are already handled and logged, ignore all others attempting to shut down.
        }
    }

    private void OnApplicationExit(object? sender, ExitEventArgs e)
    {
        Current.Exit -= OnApplicationExit;
        this.logger?.LogAlways(_ => "=========== Application Exiting ===========");
        this.host?.StopAsync().GetAwaiter().GetResult();
        this.host?.Dispose();
    }

    private void OnCurrentDomainUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        LogUnhandledException("App.OnCurrentDomainUnhandledException", e.ExceptionObject);
    }

    private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogUnhandledException("App.OnDispatcherUnhandledException", e.Exception);
    }
}
