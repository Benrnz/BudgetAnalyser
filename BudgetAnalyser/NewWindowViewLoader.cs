using System;
using Rees.Wpf;

namespace BudgetAnalyser
{
    public class NewWindowViewLoader : WpfViewLoader<NewWindowContainer>
    {
        public override void Show(object context)
        {
            if (this.TargetWindow != null)
            {
                try
                {
                    this.TargetWindow.Close();
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                    // ReSharper restore EmptyGeneralCatchClause
                {
                    // Swallow any exception trying to close the orphaned window.
                }
            }

            this.TargetWindow = new NewWindowContainer
            {
                DataContext = context,
                MainContent = { Content = context },
            };
            this.TargetWindow.Show();
        }

        public override bool? ShowDialog(object context)
        {
            throw new NotSupportedException();
        }
    }
}