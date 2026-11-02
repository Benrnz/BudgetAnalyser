using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BudgetAnalyser.Budget;

/// <summary>
///     Interaction logic for BudgetModelNameUserControl.xaml
/// </summary>
public partial class BudgetModelNameUserControl : UserControl
{
    /// <summary>
    ///     The command to rename the budget. When set, the budget name is shown as a hyperlink that executes this command, otherwise the name is read only.
    /// </summary>
    public static readonly DependencyProperty RenameCommandProperty = DependencyProperty.Register(
        nameof(RenameCommand),
        typeof(ICommand),
        typeof(BudgetModelNameUserControl),
        new PropertyMetadata(null));

    public BudgetModelNameUserControl()
    {
        InitializeComponent();
    }

    public ICommand? RenameCommand
    {
        get => (ICommand?)GetValue(RenameCommandProperty);
        set => SetValue(RenameCommandProperty, value);
    }
}
