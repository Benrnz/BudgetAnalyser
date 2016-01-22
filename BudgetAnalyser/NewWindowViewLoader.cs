using System;
using BudgetAnalyser.Engine;
using Rees.Wpf;

namespace BudgetAnalyser
{
    [AutoRegisterWithIoC]
    public class NewWindowViewLoader : WpfViewLoader<NewWindowContainer>
    {
        public override bool? ShowDialog(object context)
        {
            throw new NotSupportedException();
        }

        protected override void ConfigureWindow(object context)
        {
            base.ConfigureWindow(context);
            TargetWindow.MainContent.Content = context;
        }
    }
}