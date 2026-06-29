using System.Diagnostics.CodeAnalysis;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Engine;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Rees.Wpf;
using Rees.Wpf.Contracts;
using Rees.Wpf.UserInteraction;

namespace BudgetAnalyser;

internal static class BudgetAnalyserIocRegistrations
{
    /// <summary>
    ///    Registers all Rees.Wpf types into the given service collection configured for the BudgetAnalyser application.
    /// </summary>
    /// <param name="services">The service collection to register Rees.Wpf types into.</param>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Required here, Composition Root pattern")]
    internal static void AddReesWpfRegistrations(this IServiceCollection services)
    {
        // Registrations from Rees.Wpf - there are no automatic registrations in this assembly.
        // Wait cursor factory.
        services.AddSingleton<Func<IWaitCursor>>(() => new WpfWaitCursor());

        services.AddSingleton<IPersistApplicationState, PersistBaxAppStateAsJson>();

        // Input box: resolved by concrete type so that WindowsInputBox can receive the right IViewLoader.
        services.AddTransient<WpfViewLoader<InputBox>>();
        services.AddTransient<IUserInputBox>(sp => new WindowsInputBox(sp.GetRequiredService<WpfViewLoader<InputBox>>()));

        services.AddSingleton<IUserMessageBox, WindowsMessageBox>();
        services.AddSingleton<IUserQuestionBoxYesNo, WindowsQuestionBoxYesNo>();

        services.AddSingleton<Func<IUserPromptOpenFile>>(_ => () => new WindowsOpenFileDialog { AddExtension = true, CheckFileExists = true, CheckPathExists = true });
        services.AddSingleton<Func<IUserPromptSaveFile>>(_ => () => new WindowsSaveFileDialog { AddExtension = true, CheckPathExists = true });
    }

    /// <summary>
    ///     Supplement with explicit non-automatic registrations.
    /// </summary>
    internal static void AddBudgetAnalyserRegistrations(this IServiceCollection services)
    {
        // Override / supplement with explicit non-automatic registrations.
        // ILogger: explicit singleton so log level can be configured once at startup.
        services.AddSingleton<ILogger, DebugLogger>();

        // IUiContext: singleton ambient context used by all controllers.
        services.AddSingleton<IUiContext, UiContext>();

        // IMessenger: supply WeakReferenceMessenger.Default as the inner messenger so that controllers which rely on the default messenger in their base class share the same bus.
        // NOTE: this must be the only place WeakReferenceMessenger.Default is referenced. A factory is used here rather than auto-registration to avoid a circular dependency
        // (ConcurrentMessenger itself takes IMessenger in its constructor).
        services.AddSingleton<IMessenger>(sp => new ConcurrentMessenger(WeakReferenceMessenger.Default, sp.GetRequiredService<ILogger>()));
    }
}
