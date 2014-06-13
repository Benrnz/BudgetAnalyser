using System;
using Rees.Wpf;

namespace BudgetAnalyser
{
    public class NewWindowViewLoader : WpfViewLoader<NewWindowContainer>
    {
        protected override void ConfigureWindow(object context)
        {
            base.ConfigureWindow(context);
            TargetWindow.MainContent.Content = context;
        }

        public override bool? ShowDialog(object context)
        {
            throw new NotSupportedException();
        }
    }
}