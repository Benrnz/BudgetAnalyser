using System;
using Rees.Wpf;

namespace BudgetAnalyser
{
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