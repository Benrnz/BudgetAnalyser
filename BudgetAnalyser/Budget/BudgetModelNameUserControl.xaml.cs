using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BudgetAnalyser.Budget
{
    /// <summary>
    ///     Interaction logic for BudgetModelNameUserControl.xaml
    /// </summary>
    public partial class BudgetModelNameUserControl : UserControl
    {
        public BudgetModelNameUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Finalise editing of the Budget Name.
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.budgetNameTextBox.Visibility = Visibility.Hidden;
            Debug.Assert(this.budgetNameTextBlock != null);
            this.budgetNameTextBlock.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Enable editing of the Budget Name
        /// </summary>
        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var control = e.OriginalSource as TextBlock;
            if (!Equals(control, this.budgetNameTextBlock))
            {
                return;
            }

            this.budgetNameTextBox.Visibility = Visibility.Visible;
            Debug.Assert(this.budgetNameTextBlock != null);
            this.budgetNameTextBlock.Visibility = Visibility.Hidden;
        }
    }
}