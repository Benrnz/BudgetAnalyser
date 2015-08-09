using System.Windows.Controls;
using System.Windows.Input;

namespace BudgetAnalyser.Statement
{
    /// <summary>
    ///     Interaction logic for LoadFileView.xaml
    /// </summary>
    public partial class LoadFileView : UserControl
    {
        public LoadFileView()
        {
            InitializeComponent();
        }

private  LoadFileController Controller => (LoadFileController)DataContext;

        private void OnFileNameMouseUp(object sender, MouseEventArgs e)
        {
            Controller.BrowseForFileCommand.Execute(null);
        }
    }
}