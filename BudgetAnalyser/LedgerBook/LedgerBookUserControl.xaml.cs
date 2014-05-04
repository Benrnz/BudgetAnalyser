using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook
{
    /// <summary>
    ///     Interaction logic for LegderBookView.xaml
    /// </summary>
    public partial class LedgerBookUserControl
    {
        private const string BankBalanceBackground = "Brush.TileBackgroundAlternate";
        private const string BankBalanceTextBrush = "Brush.DefaultText";
        private const string DateFormat = "d-MMM-yy";
        private const string HeadingStyle = "LedgerBookTextBlockHeading";
        private const string ImportantNumberStyle = "LedgerBookTextBlockImportantNumber";
        private const string LightBorderBrush = "Brush.BorderLight";

        private const string NormalHighlightBackground = "Brush.TileBackground";
        private const string NormalStyle = "LedgerBookTextBlockOther";
        private const string NumberStyle = "LedgerBookTextBlockNumber";
        private const string SurplusBackground = "Brush.TileBackgroundAlternate";
        private const string SurplusTextBrush = "Brush.CreditBackground1";
        private static readonly GridLength MediumGridWidth = new GridLength(100);
        private static readonly GridLength SmallGridWidth = new GridLength(60);

        private bool subscribedToControllerPropertyChanged;
        private bool subscribedToMainWindowClose;

        public LedgerBookUserControl()
        {
            InitializeComponent();
        }

        private LedgerBookController Controller
        {
            get { return DataContext as LedgerBookController; }
        }

        private Border AddBorderToGridCell(Panel parent, bool hasBackground, bool hasBorder, int column, int row)
        {
            return AddBorderToGridCell(parent, hasBackground ? NormalHighlightBackground : null, hasBorder, column, row);
        }

        private Border AddBorderToGridCell(Panel parent, string background, bool hasBorder, int column, int row)
        {
            var border = new Border();
            if (background != null)
            {
                border.Background = FindResource(background) as Brush;
            }

            if (hasBorder)
            {
                border.BorderBrush = FindResource(LightBorderBrush) as Brush;
                border.BorderThickness = new Thickness(0, 0, 1, 0);
            }

            Grid.SetColumn(border, column);
            Grid.SetRow(border, row);
            parent.Children.Add(border);
            return border;
        }

        private TextBlock AddContentToGrid(FrameworkElement parent, string content, ref int column, int row, string style, string tooltip = null)
        {
            var panel = parent as Panel;
            var decorator = parent as Decorator;

            var textBlock = new TextBlock
            {
                Style = FindResource(style) as Style,
                Text = content,
                ToolTip = tooltip ?? content,
            };
            Grid.SetColumn(textBlock, column++);
            Grid.SetRow(textBlock, row);
            if (panel != null)
            {
                panel.Children.Add(textBlock);
            }
            else if (decorator != null)
            {
                decorator.Child = textBlock;
            }
            else
            {
                throw new ArgumentException("parent is not a Panel nor a Decorator", "parent");
            }

            return textBlock;
        }

        private void AddHeadingRow(Grid grid)
        {
            int column = 0;
            Border dateBorder = AddBorderToGridCell(grid, true, true, column, 0);
            AddContentToGrid(dateBorder, "Date", ref column, 0, HeadingStyle);

            foreach (Ledger ledger in Controller.LedgerBook.Ledgers)
            {
                Border border = AddBorderToGridCell(grid, true, true, column, 0);
                // SpentMonthly Legders do not show the transaction total (NetAmount) because its always the same.
                Grid.SetColumnSpan(border, ledger.BudgetBucket is SpentMonthlyExpense ? 1 : 2);
                string tooltip = string.Format(CultureInfo.CurrentCulture, "{0}: {1} - {2}", ledger.BudgetBucket.TypeDescription, ledger.BudgetBucket.Code, ledger.BudgetBucket.Description);
                TextBlock ledgerTitle = AddContentToGrid(border, ledger.BudgetBucket.Code, ref column, 0, HeadingStyle, tooltip);
                ledgerTitle.HorizontalAlignment = HorizontalAlignment.Center;
                column--; // Ledger heading shares a column with other Ledger Headings

                if (!(ledger.BudgetBucket is SpentMonthlyExpense))
                {
                    TextBlock ledgerTxnsHeading = AddContentToGrid(grid, "Txns", ref column, 0, HeadingStyle, "Transactions");
                    ledgerTxnsHeading.Margin = new Thickness(5, 30, 5, 2);
                    ledgerTxnsHeading.HorizontalAlignment = HorizontalAlignment.Right;
                }

                TextBlock ledgerBalanceHeading = AddContentToGrid(grid, "Balance", ref column, 0, HeadingStyle, "Ledger Balance");
                ledgerBalanceHeading.Margin = new Thickness(5, 30, 5, 2);
                ledgerBalanceHeading.HorizontalAlignment = HorizontalAlignment.Right;
            }

            Border surplusBorder = AddBorderToGridCell(grid, SurplusBackground, false, column, 0);
            TextBlock surplusTextBlock = AddContentToGrid(surplusBorder, "Surplus", ref column, 0, HeadingStyle);
            surplusTextBlock.Foreground = FindResource(SurplusTextBrush) as Brush;
            surplusTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            Border adjustmentsBorder = AddBorderToGridCell(grid, true, false, column, 0);
            TextBlock adjustmentsTextBlock = AddContentToGrid(adjustmentsBorder, "Adjustments", ref column, 0, HeadingStyle);
            adjustmentsTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            Border bankBalanceBorder = AddBorderToGridCell(grid, BankBalanceBackground, false, column, 0);
            TextBlock bankBalanceTextBlock = AddContentToGrid(bankBalanceBorder, "Balance", ref column, 0, HeadingStyle);
            bankBalanceTextBlock.Foreground = FindResource(BankBalanceTextBrush) as Brush;
            bankBalanceTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            AddBorderToGridCell(grid, true, false, column, 0);
            AddContentToGrid(grid, "Remarks", ref column, 0, HeadingStyle);
        }

        private TextBlock AddHyperlinkToGrid(Panel parent, string content, ref int column, int row, string style, string tooltip = null, object parameter = null)
        {
            var hyperlink = new Hyperlink(new Run(content))
            {
                Command = Controller.ShowTransactionsCommand,
                CommandParameter = parameter,
            };
            var textBlock = new TextBlock(hyperlink)
            {
                Style = FindResource(style) as Style,
                ToolTip = tooltip ?? content,
            };
            Grid.SetColumn(textBlock, column++);
            Grid.SetRow(textBlock, row);
            parent.Children.Add(textBlock);
            return textBlock;
        }

        private void AddLedgerColumns(Grid grid)
        {
            foreach (var ledger in Controller.LedgerBook.Ledgers)
            {
                if (ledger.BudgetBucket is SpentMonthlyExpense)
                {
                    // Spent Monthly ledgers only have a balance column
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = SmallGridWidth });
                }
                else
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = SmallGridWidth });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = SmallGridWidth });
                }
            }
        }

        private void AddLedgerEntryLines(Grid grid)
        {
            int row = 1;
            List<Ledger> allLedgers = Controller.LedgerBook.Ledgers.ToList();
            foreach (LedgerEntryLine line in Controller.LedgerBook.DatedEntries)
            {
                if (row >= 13)
                {
                    // Only showing 1 year worth of data for now.
                    break;
                }

                int column = 0;
                Border dateBorder = AddBorderToGridCell(grid, false, true, column, row);
                AddContentToGrid(dateBorder, line.Date.ToString(DateFormat, CultureInfo.CurrentCulture), ref column, row, NormalStyle);

                foreach (Ledger ledger in allLedgers)
                {
                    LedgerEntry entry = line.Entries.FirstOrDefault(e => e.Ledger.Equals(ledger));
                    decimal balance, netAmount;

                    if (entry == null)
                    {
                        // New ledger added that older entries do not have.
                        balance = 0;
                        netAmount = 0;
                    }
                    else
                    {
                        balance = entry.Balance;
                        netAmount = entry.NetAmount;
                    }

                    if (ledger.BudgetBucket is SpentMonthlyExpense)
                    {
                        AddBorderToGridCell(parent: grid, hasBackground: false, hasBorder: true, column: column, row: row);
                        AddHyperlinkToGrid(grid, balance.ToString("N", CultureInfo.CurrentCulture), ref column, row, NumberStyle, parameter: entry);
                    }
                    else
                    {
                        AddBorderToGridCell(parent: grid, hasBackground: true, hasBorder: false, column: column, row: row);
                        AddHyperlinkToGrid(grid, netAmount.ToString("N", CultureInfo.CurrentCulture), ref column, row, NumberStyle, parameter: entry);
                        AddBorderToGridCell(parent: grid, hasBackground: false, hasBorder: true, column: column, row: row);
                        AddHyperlinkToGrid(grid, balance.ToString("N", CultureInfo.CurrentCulture), ref column, row, NumberStyle, parameter: entry);
                    }
                }

                Border surplusBorder = AddBorderToGridCell(grid, SurplusBackground, false, column, row);
                TextBlock surplusText = AddContentToGrid(surplusBorder, line.CalculatedSurplus.ToString("N", CultureInfo.CurrentCulture), ref column, row, ImportantNumberStyle);
                surplusText.Foreground = FindResource(SurplusTextBrush) as Brush;

                AddHyperlinkToGrid(grid, line.TotalBalanceAdjustments.ToString("N", CultureInfo.CurrentCulture), ref column, row, ImportantNumberStyle, parameter: line);

                Border bankBalanceBorder = AddBorderToGridCell(grid, BankBalanceBackground, false, column, row);
                TextBlock bankBalanceText = AddContentToGrid(bankBalanceBorder, line.LedgerBalance.ToString("N", CultureInfo.CurrentCulture), ref column, row, ImportantNumberStyle,
                    string.Format(CultureInfo.CurrentCulture, "Ledger Balance: {0:N} Bank Balance {1:N}", line.LedgerBalance, line.BankBalance));
                bankBalanceText.Foreground = FindResource(BankBalanceTextBrush) as Brush;

                TextBlock remarksHyperlink = AddHyperlinkToGrid(grid, "...", ref column, row, NormalStyle, line.Remarks);
                var hyperlink = (Hyperlink)remarksHyperlink.Inlines.FirstInline;
                hyperlink.Command = Controller.ShowRemarksCommand;
                hyperlink.CommandParameter = line;

                row++;
            }
        }

        private void AddLedgerRows(Grid grid)
        {
            for (int index = 0; index <= Controller.LedgerBook.DatedEntries.Count(); index++)
            {
                // <= because we must allow one row for the heading.
                if (index >= 12)
                {
                    // Maximum of 12 months + 1 heading row
                    break;
                }

                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
        }

        /// <summary>
        ///     This is drawn programatically because the dimensions of the ledger book grid are two-dimensional and dynamic.
        ///     Unknown number
        ///     of columns and many rows. ListView and DataGrid dont work well.
        /// </summary>
        private void DynamicallyCreateLedgerBookGrid()
        {
            if (Controller.LedgerBook == null)
            {
                this.LedgerBookPanel.Content = null;
                return;
            }

            var grid = new Grid();
            // Date
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = MediumGridWidth });
            // Ledgers 
            AddLedgerColumns(grid);
            // Surplus
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = MediumGridWidth });
            // Adjustments
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = MediumGridWidth });
            // Ledger Balance
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = MediumGridWidth });
            // Remarks
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = SmallGridWidth });

            this.LedgerBookPanel.Content = grid;
            AddLedgerRows(grid);
            AddHeadingRow(grid);
            AddLedgerEntryLines(grid);
        }

        private object FindResource(string resourceName)
        {
            object localResource = Resources[resourceName];
            if (localResource != null)
            {
                return localResource;
            }

            return Application.Current.FindResource(resourceName);
        }

        private void OnControllerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LedgerBook")
            {
                DynamicallyCreateLedgerBookGrid();
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!this.subscribedToMainWindowClose)
            {
                this.subscribedToMainWindowClose = true;
                Application.Current.MainWindow.Closing += OnMainWindowClosing;
            }

            if (this.subscribedToControllerPropertyChanged && e.OldValue != null)
            {
                Controller.PropertyChanged -= OnControllerPropertyChanged;
            }
            else if (!this.subscribedToControllerPropertyChanged && e.NewValue != null)
            {
                this.subscribedToControllerPropertyChanged = true;
                Controller.PropertyChanged += OnControllerPropertyChanged;
            }

            if (e.OldValue != null)
            {
                ((LedgerBookController)e.OldValue).LedgerBookUpdated -= OnLedgerBookUpdated;
            }

            if (e.NewValue != null)
            {
                ((LedgerBookController)e.NewValue).LedgerBookUpdated += OnLedgerBookUpdated;
            }

            DynamicallyCreateLedgerBookGrid();
        }

        private void OnLedgerBookNameClick(object sender, MouseButtonEventArgs e)
        {
            Controller.EditLedgerBookName();
        }

        private void OnLedgerBookUpdated(object sender, EventArgs e)
        {
            ResetLedgerBookContent();
            DynamicallyCreateLedgerBookGrid();
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            Controller.NotifyOfClosing();
        }

        private void ResetLedgerBookContent()
        {
            this.LedgerBookPanel.Content = null;
        }
    }
}