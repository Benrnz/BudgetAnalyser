using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using BudgetAnalyser.Engine.Matching;

namespace BudgetAnalyser.Matching
{
    /// <summary>
    ///     Interaction logic for EditRulesUserControl.xaml
    /// </summary>
    public partial class EditRulesUserControl : UserControl
    {
        private bool backgroundUpdate;
        private string currentSort;

        public EditRulesUserControl()
        {
            InitializeComponent();
        }

        private RulesController Controller => (RulesController)DataContext;

        private void OnCheckedAndSelectedRule(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null)
            {
                return;
            }

            if (radioButton.IsChecked is not null && radioButton.IsChecked.Value)
            {
                return;
            }

            this.OrButton.IsChecked = true;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is not null)
            {
                var oldController = (RulesController)e.OldValue;
                oldController.SortChanged -= OnSortChanged;
                oldController.RuleRemoved -= OnRuleRemoved;
                oldController.RuleAdded -= OnRuleAdded;
            }

            if (e.NewValue is not null)
            {
                Controller.SortChanged += OnSortChanged;
                Controller.RuleRemoved += OnRuleRemoved;
                Controller.RuleAdded += OnRuleAdded;
                this.currentSort = Controller.SortBy;
            }
        }

        private void OnGroupListBoxLoaded(object sender, RoutedEventArgs e)
        {
            var listBox = e.OriginalSource as ListBox;
            if (listBox == null)
            {
                return;
            }

            Controller.PropertyChanged += (s, eventArgs) =>
            {
                // Ensure all other listboxes deselect their currently selected item when another is chosen in different listbox.
                if (eventArgs.PropertyName == "SelectedRule")
                {
                    Debug.WriteLine("EditRulesUserControl: SelectedRule Changed handled by a listbox");
                    if (listBox.SelectedItem == null || Controller.SelectedRule == null)
                    {
                        return;
                    }

                    if (listBox.SelectedItem.Equals(Controller.SelectedRule))
                    {
                        return;
                    }

                    this.backgroundUpdate = true;
                    listBox.SelectedItem = null;
                    this.backgroundUpdate = false;
                }
            };
        }

        private void OnGroupListBoxRuleSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.backgroundUpdate)
            {
                return;
            }

            // Must account for when rules are deleted - this method gets called for adding and deleting to the selection.
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var selectedRule = (MatchingRule)e.AddedItems[0];

            // Chosen not to use Data Binding here.  The rule list box inside the group data template does not deselect the selected item when a different 
            // rule is chosen in a different group.  This is a big problem, because if code is written to deselect it manually (set it to null), this triggers data binding
            // to update the controller, which then deselects the rule the user has just chosen.
            Controller.SelectedRule = selectedRule;
        }

        private void OnRuleAdded(object sender, MatchingRuleEventArgs e)
        {
            MatchingRule rule = e.Rule;
            var flatList = (ObservableCollection<MatchingRule>)this.FlatListBox.ItemsSource;
            if (flatList.All(r => r.RuleId != rule.RuleId))
            {
                flatList.Add(rule);
            }

            var groupedList = (ObservableCollection<RulesGroupedByBucket>)this.GroupedByListBox.ItemsSource;
            RulesGroupedByBucket group = groupedList.SingleOrDefault(g => g.Bucket == rule.Bucket);
            if (group == null)
            {
                group = new RulesGroupedByBucket(rule.Bucket, new[] { rule });
                groupedList.Add(group);
            }

            if (group.Rules.All(r => r.RuleId != rule.RuleId))
            {
                group.Rules.Add(rule);
            }
        }

        private void OnRuleRemoved(object sender, EventArgs e)
        {
            var rule = (MatchingRule)sender;
            var flatList = (ObservableCollection<MatchingRule>)this.FlatListBox.ItemsSource;
            flatList.Remove(rule);

            var groupedList = (ObservableCollection<RulesGroupedByBucket>)this.GroupedByListBox.ItemsSource;
            RulesGroupedByBucket group = groupedList.FirstOrDefault(g => g.Bucket == rule.Bucket);
            if (group is not null)
            {
                group.Rules.Remove(rule);
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
                            ICollectionView view1 = CollectionViewSource.GetDefaultView(this.FlatListBox.ItemsSource);
                            view1.SortDescriptions.Clear();
                            view1.SortDescriptions.Add(new SortDescription("Description", ListSortDirection.Ascending));
                            view1.Refresh();
                            break;

                        case RulesController.MatchesSortKey:
                            ICollectionView view2 = CollectionViewSource.GetDefaultView(this.FlatListBox.ItemsSource);
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
                    ICollectionView view = CollectionViewSource.GetDefaultView(this.GroupedByListBox.ItemsSource);
                    view.Refresh();
                }
            }
        }

        private void OnVisibilityChangedAndButton(object sender, DependencyPropertyChangedEventArgs e)
        {
            OnCheckedAndSelectedRule(sender, new RoutedEventArgs());
        }
    }
}