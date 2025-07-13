using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BudgetAnalyser.Engine;
using Microsoft.Extensions.Hosting;

namespace BudgetAnalyser;

[SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This is the root object in the App")]
public partial class App
{
    private readonly CompositionRoot compositionRoot; // TODO no need for this under MS DI. Disposal is handled by the host.
    private readonly ILogger logger;
    private readonly ShellController shellController;

    public App()
    {
        this.compositionRoot = new CompositionRoot();

        // TODO Ideally this should be in the CompositionRoot
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // this.compositionRoot = new CompositionRoot(services);
                // All registrations go in here
                // services.AddBudgetAnalyserEngineServices();
                // services.AddBudgetAnalyserServices();
            })
            .Build();

        this.compositionRoot.Build();
        this.logger = this.compositionRoot.Logger;
        this.shellController = this.compositionRoot.ShellController;
    }

    public static IHost AppHost { get; private set; } = null!;

    // ReSharper disable once AsyncVoidMethod - Override of OnExit in WPF applications must be async void.
    protected override async void OnExit(ExitEventArgs e)
    {
        // Occurs before Application Exit event - calling base will raise the event.
        await AppHost.StopAsync();
        AppHost.Dispose();
        base.OnExit(e);
    }

    // ReSharper disable once AsyncVoidMethod - Override of OnStartup in WPF applications must be async void.
    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost.StartAsync();
        base.OnStartup(e);

        // Ensure the current culture passed into bindings is the OS culture. By default, WPF uses en-US as the culture, regardless of the system settings.
        FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

        Current.Exit += OnApplicationExit;

        this.logger.LogAlways(_ => "=========== Budget Analyser is Starting ===========");
        this.logger.LogAlways(_ => this.compositionRoot.ShellController.TabDashboardController.VersionString);

        this.shellController.Initialize();

        this.logger.LogInfo(_ => "Initialisation finished.");

        var topLevelWindow = new ShellWindow { DataContext = this.shellController };
        topLevelWindow.Show();
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
        Justification = "All exceptions are already logged, if there are any further exceptions when attempting to gracefully shutdown, they can be ignored.")]
    private void LogUnhandledException(string origin, object ex)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Empty);
        builder.AppendLine("=====================================================================================");
        builder.AppendLine(DateTime.Now.ToString(CultureInfo.CurrentCulture));
        builder.AppendLine("Unhandled exception was thrown from origin: " + origin);
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

    private void OnApplicationExit(object? sender, ExitEventArgs e)
    {
        Current.Exit -= OnApplicationExit;
        this.compositionRoot.Dispose();
        AppHost.Dispose();
        this.logger.LogAlways(_ => "=========== Application Exiting ===========");
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
