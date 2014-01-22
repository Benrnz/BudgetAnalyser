using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    /// Interaction logic for AddLedgerReconciliationUserControl.xaml
    /// </summary>
    public partial class AddLedgerReconciliationUserControl : UserControl
    {
        public AddLedgerReconciliationUserControl()
        {
            InitializeComponent();
        }

        private void OnUIElementGotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }
    }
}
