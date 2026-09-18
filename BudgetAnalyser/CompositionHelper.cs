using System.Diagnostics;
using System.Windows;
using BudgetAnalyser.ApplicationState;
using BudgetAnalyser.Encryption;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Transactions;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetAnalyser;

/// <summary>
///     Composition root helper methods for service registration and object graph bootstrapping.
/// </summary>
public static class CompositionHelper
{
    private static bool ControllersInitialised;

    /// <summary>
    ///     Build and initialise late-bound parts of the object graph that require a built provider.
    /// </summary>
    public static void BuildApplicationObjectGraph(IServiceProvider provider)
    {
        if (provider is null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        var isMainThread = Application.Current.Dispatcher.CheckAccess();
        Debug.Assert(isMainThread, "CompositionHelper.BuildApplicationObjectGraph must be called on the main UI thread.");

        var engineAssembly = typeof(TransactionsListModel).Assembly;
        var encryptionAssembly = typeof(IFileEncryptor).Assembly;
        var thisAssembly = typeof(CompositionHelper).Assembly;

        // Perform property injection for static classes that need it.
        // Property injection is a last resort, used only where data binding to static properties requires it.
        foreach (var assembly in new[] { engineAssembly, thisAssembly, encryptionAssembly })
        {
            var requiredPropertyInjections = EngineIocRegistrations.ProcessPropertyInjection(assembly);
            foreach (var requirement in requiredPropertyInjections)
            {
                var dependency = provider.GetService(requirement.Type);
                if (dependency is not null)
                {
                    requirement.PropertyInjectionAssignment(dependency);
                }
            }
        }
    }

    /// <summary>
    ///     Load application state into controllers.
    /// </summary>
    public static void LoadApplicationStateIntoControllers(ILogger logger, IPersistApplicationState statePersistence, IMessenger messenger)
    {
        if (ControllersInitialised)
        {
            return;
        }

        logger.LogInfo(_ => $"ShellController Initialise started. {DateTime.Now}");
        ControllersInitialised = true;

        IList<IPersistentApplicationStateObject> rehydratedState = statePersistence.Load().ToList();
        if (rehydratedState.None())
        {
            rehydratedState = [];
        }

        // Send state load messages in order, grouped by sequence.
        var sequenceGroups = rehydratedState.GroupBy(persistentModel => persistentModel.LoadSequence).OrderBy(group => group.Key);
        foreach (var models in sequenceGroups)
        {
            logger.LogInfo(_ => $"ShellController sending ApplicationStateLoadedMessage for: Sequence{models.Key} {models.First().GetType().Name}");
            messenger.Send(new ApplicationStateLoadedMessage(models));
        }

        logger.LogInfo(_ => $"ShellController Initialise completing. Sending ApplicationStateLoadFinishedMessage. {DateTime.Now}");
        messenger.Send(new ApplicationStateLoadFinishedMessage());
    }
}
