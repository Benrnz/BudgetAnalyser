using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private string currentSort;

        public EditRulesUserControl()
        {
            InitializeComponent();
        }

        private RulesController Controller
        {
            get { return (RulesController)DataContext; }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var oldController = (RulesController)e.OldValue;
                oldController.SortChanged -= OnSortChanged;
                oldController.RuleRemoved -= OnRuleRemoved;
                oldController.RuleAdded -= OnRuleAdded;
            }

            if (e.NewValue != null)
            {
                Controller.SortChanged += OnSortChanged;
                Controller.RuleRemoved += OnRuleRemoved;
                Controller.RuleAdded += OnRuleAdded;
                this.currentSort = Controller.SortBy;
            }
        }

        private void OnRuleAdded(object sender, EventArgs e)
        {
            var rule = (MatchingRule)sender;
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
            RulesGroupedByBucket group = groupedList.Single(g => g.Bucket == rule.Bucket);
            group.Rules.Remove(rule);
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
    }
}