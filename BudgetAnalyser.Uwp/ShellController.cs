using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Persistence;
using BudgetAnalyser.Engine.Services;
using BudgetAnalyser.Engine.Widgets;
using BudgetAnalyser.Uwp.ApplicationState;
using JetBrains.Annotations;

namespace BudgetAnalyser.Uwp
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ShellController : GalaSoft.MvvmLight.ViewModelBase
    {
        private readonly UiContext uiContext;
        private readonly IDashboardService dashboardService;
        private readonly IPersistApplicationState statePersistence;
        //private readonly PersistenceOperations persistenceOperations;
        private bool initialised;

        public ShellController([NotNull] UiContext uiContext, [NotNull] IDashboardService dashboardService, IPersistApplicationState statePersistenceProvider)
        {
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (dashboardService == null) throw new ArgumentNullException(nameof(dashboardService));

            this.uiContext = uiContext;
            this.dashboardService = dashboardService;
            this.statePersistence = statePersistenceProvider;
            // this.persistenceOperations = persistenceOperations;
        }

        public async Task InitialiseAsync()
        {
            if (this.initialised) return;
            this.initialised = true;
            this.uiContext.Controllers.OfType<IInitialisableController>().ToList().ForEach(i => i.Initialise());

            IList<IPersistentApplicationStateObject> rehydratedModels = (await this.statePersistence.LoadAsync())?.ToList();
            if (rehydratedModels == null || rehydratedModels.None())
            {
                rehydratedModels = CreateNewDefaultApplicationState();
            }

            // Create a distinct list of sequences.
            IEnumerable<int> sequences = rehydratedModels.Select(persistentModel => persistentModel.LoadSequence).OrderBy(s => s).Distinct();

            // Send state load messages in order.
            foreach (int sequence in sequences)
            {
                int sequenceCopy = sequence;
                IEnumerable<IPersistentApplicationStateObject> models = rehydratedModels.Where(persistentModel => persistentModel.LoadSequence == sequenceCopy);
                MessengerInstance.Send(new ApplicationStateLoadedMessage(models));
            }

            MessengerInstance.Send(new ApplicationStateLoadFinishedMessage());
        }

        private static IList<IPersistentApplicationStateObject> CreateNewDefaultApplicationState()
        {
            // Widget persistent state object is required to draw the widgets, even the ones that should be there by default.
            // The widgets must be drawn so a user can open or create a new file. 
            var appState = new List<IPersistentApplicationStateObject>
            {
                new WidgetsApplicationState()
            };
            return appState;
        }
    }
}
