using System;
using BudgetAnalyser.Engine;
using BudgetAnalyser.Engine.Services;
using JetBrains.Annotations;

namespace BudgetAnalyser.Uwp
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ShellController : GalaSoft.MvvmLight.ViewModelBase
    {
        private readonly UiContext uiContext;
        private readonly IDashboardService dashboardService;
        // private readonly IPersistApplicationState statePersistence;
        //private readonly PersistenceOperations persistenceOperations;

        public ShellController([NotNull] UiContext uiContext, [NotNull] IDashboardService dashboardService)
        {
            if (uiContext == null) throw new ArgumentNullException(nameof(uiContext));
            if (dashboardService == null) throw new ArgumentNullException(nameof(dashboardService));

            this.uiContext = uiContext;
            this.dashboardService = dashboardService;
            // this.statePersistence = statePersistence;
            // this.persistenceOperations = persistenceOperations;
        }
    }
}
