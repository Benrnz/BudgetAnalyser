using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.Dashboard
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class DashboardController : ControllerBase, IShowableController
    {
        private bool doNotUseShown;

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => Shown);
            }
        }
    }
}
