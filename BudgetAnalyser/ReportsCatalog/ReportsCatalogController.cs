using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser.ReportsCatalog
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class ReportsCatalogController : ControllerBase, IShowableController
    {
        private bool doNotUseShown;

        public bool Shown
        {
            get { return this.doNotUseShown; }
            set
            {
                this.doNotUseShown = value;
                RaisePropertyChanged(() => this.Shown);
            }
        }
    }
}
