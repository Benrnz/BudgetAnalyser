using System.Collections.Generic;
using BudgetAnalyser.Engine;
using GalaSoft.MvvmLight;

namespace BudgetAnalyser.Uwp
{
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class UiContext
    {
        public virtual ILogger Logger { get; set; }

        public IEnumerable<ViewModelBase> Controllers
        {
            get { return new List<ViewModelBase>(); }
        }

        public ShellController ShellController { get; set; }
    }
}
