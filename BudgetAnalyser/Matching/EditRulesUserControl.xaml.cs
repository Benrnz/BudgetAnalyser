using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Rees.Wpf;

namespace BudgetAnalyser.Matching
{
    /// <summary>
    ///     Interaction logic for EditRulesUserControl.xaml
    /// </summary>
    public partial class EditRulesUserControl : UserControl
    {
        private string currentSort = null;

        public EditRulesUserControl()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var oldController = (RulesController)e.OldValue;
                oldController.SortChanged -= OnSortChanged;
            }

            if (e.NewValue != null)
            {
                Controller.SortChanged += OnSortChanged;
                this.currentSort = Controller.SortBy;
            }
        }

        private void OnSortChanged(object sender, EventArgs e)
        {
            if (Controller.SortBy != this.currentSort)
            {
                this.currentSort = Controller.SortBy;
                if (Controller.FlatListBoxVisibility)
                {
                    switch (this.currentSort)
                    {
                        case RulesController.DescriptionSortKey:
                            var view1 = CollectionViewSource.GetDefaultView(FlatListBox.ItemsSource);
                            view1.SortDescriptions.Clear();
                            view1.SortDescriptions.Add(new SortDescription("Description", ListSortDirection.Ascending));
                            view1.Refresh();
                            break;

                        case RulesController.MatchesSortKey:
                            var view2 = CollectionViewSource.GetDefaultView(FlatListBox.ItemsSource);
                            view2.SortDescriptions.Clear();
                            view2.SortDescriptions.Add(new SortDescription("MatchCount", ListSortDirection.Descending));
                            view2.Refresh();
                            break;

                        default:
                            throw new ArgumentException("Invalid sort value: " + this.currentSort);
                    }
                }

                if (Controller.GroupByListBoxVisibility)
                {
                    var view = CollectionViewSource.GetDefaultView(GroupedByListBox.ItemsSource);
                    view.Refresh();
                }
            }
        }

        private RulesController Controller
        {
            get { return (RulesController)DataContext; }
        }
    }
}