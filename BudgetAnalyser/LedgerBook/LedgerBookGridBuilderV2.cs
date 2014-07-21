using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook
{
    public class LedgerBookGridBuilderV2 : ILedgerBookGridBuilder
    {
        private const string BankBalanceBackground = "Brush.TileBackgroundAlternate";
        private const string BankBalanceTextBrush = "Brush.Text.Default";
        private const string DateColumnStyle = "LedgerBookTextBlockHeadingRight";
        private const string DateFormat = "d-MMM-yy";
        private const string HeadingStyle = "LedgerBookTextBlockHeading";
        private const string ImportantNumberStyle = "LedgerBookTextBlockImportantNumber";
        private const string LightBorderBrush = "Brush.BorderLight";

        private const string NormalHighlightBackground = "Brush.TileBackground";
        private const string NormalStyle = "LedgerBookTextBlockOther";
        private const string NumberStyle = "LedgerBookTextBlockNumber";
        private const string RemarksStyle = "LedgerBookTextBlockHeadingRight";
        private const string SurplusBackground = "Brush.TileBackgroundAlternate";
        private const string SurplusTextBrush = "Brush.CreditBackground1";
        private readonly ICommand removeLedgerEntryLineCommand;
        private readonly ICommand showBankBalancesCommand;
        private readonly ICommand showRemarksCommand;
        private readonly ICommand showTransactionsCommand;
        private ContentPresenter contentPresenter;
        private Engine.Ledger.LedgerBook ledgerBook;
        private ResourceDictionary localResources;

        public LedgerBookGridBuilderV2(
            ICommand showTransactionsCommand,
            ICommand showBankBalancesCommand,
            ICommand showRemarksCommand,
            ICommand removeLedgerEntryLineCommand)
        {
            this.showTransactionsCommand = showTransactionsCommand;
            this.showBankBalancesCommand = showBankBalancesCommand;
            this.showRemarksCommand = showRemarksCommand;
            this.removeLedgerEntryLineCommand = removeLedgerEntryLineCommand;
        }

        /// <summary>
        ///     This is drawn programatically because the dimensions of the ledger book grid are two-dimensional and dynamic.
        ///     Unknown number
        ///     of columns and many rows. ListView and DataGrid dont work well.
        /// </summary>
        public void BuildGrid([CanBeNull] Engine.Ledger.LedgerBook currentLedgerBook, [NotNull] ResourceDictionary viewResources, [NotNull] ContentPresenter contentPanel)
        {
            if (viewResources == null)
            {
                throw new ArgumentNullException("viewResources");
            }

            if (contentPanel == null)
            {
                throw new ArgumentNullException("contentPanel");
            }

            this.ledgerBook = currentLedgerBook;
            this.localResources = viewResources;
            this.contentPresenter = contentPanel;
            DynamicallyCreateLedgerBookGrid();
        }

        private static Brush StripColour(LedgerColumn ledger)
        {
            if (ledger.BudgetBucket is SpentMonthlyExpenseBucket)
            {
                return ConverterHelper.SpentMonthlyBucketBrush;
            }

            if (ledger.BudgetBucket is SavedUpForExpenseBucket)
            {
                return ConverterHelper.AccumulatedBucketBrush;
            }

            if (ledger.BudgetBucket is SavingsCommitmentBucket)
            {
                return ConverterHelper.SavingsCommitmentBucketBrush;
            }

            return ConverterHelper.TileBackgroundBrush;
        }

        private Border AddBorderToGridCell(Panel parent, bool hasBackground, bool hasBorder, int gridRow, int gridColumn)
        {
            return AddBorderToGridCell(parent, hasBackground ? NormalHighlightBackground : null, hasBorder, gridRow, gridColumn);
        }

        private Border AddBorderToGridCell(Panel parent, string background, bool hasBorder, int gridRow, int gridColumn)
        {
            var border = new Border();
            if (background != null)
            {
                border.Background = FindResource(background) as Brush;
            }

            if (hasBorder)
            {
                border.BorderBrush = FindResource(LightBorderBrush) as Brush;
                border.BorderThickness = new Thickness(0, 0, 0, 1);
            }

            Grid.SetRow(border, gridRow);
            Grid.SetColumn(border, gridColumn);
            parent.Children.Add(border);
            return border;
        }

        private TextBlock AddContentToGrid(FrameworkElement parent, string content, ref int gridRow, int gridColumn, string style, string tooltip = null)
        {
            var panel = parent as Panel;
            var decorator = parent as Decorator;

            var textBlock = new TextBlock
            {
                Style = FindResource(style) as Style,
                Text = content,
                ToolTip = tooltip ?? content,
            };
            Grid.SetColumn(textBlock, gridColumn);
            Grid.SetRow(textBlock, gridRow++);
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

        private int AddDateCellToLedgerEntryLine(Grid grid, int gridRow, ref int gridColumn, LedgerEntryLine line)
        {
            Border dateBorder = AddBorderToGridCell(grid, false, true, gridRow, gridColumn);
            AddContentToGrid(dateBorder, line.Date.ToString(DateFormat, CultureInfo.CurrentCulture), ref gridRow, gridColumn, DateColumnStyle);
            gridRow--; // Not finished adding content to this cell yet.
            var button = new Button
            {
                Style = Application.Current.Resources["Button.Round.SmallCross"] as Style,
                HorizontalAlignment = HorizontalAlignment.Right,
                Command = this.removeLedgerEntryLineCommand,
                CommandParameter = line,
            };
            var visibilityBinding = new Binding("IsEnabled")
            {
                Converter = (IValueConverter)Application.Current.Resources["Converter.BoolToVis"],
                RelativeSource = new RelativeSource(RelativeSourceMode.Self),
            };
            button.SetBinding(UIElement.VisibilityProperty, visibilityBinding);

            grid.Children.Add(button);
            Grid.SetColumn(button, gridColumn);
            Grid.SetRow(button, gridRow++);

            return gridRow;
        }

        private void AddGridColumns(Grid grid)
        {
            for (int index = 0; index < this.ledgerBook.DatedEntries.Count() + 2; index++)
            {
                // + 2 because we need 2 columns for the headings
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }
        }

        private void AddHeadingColumnContent(Grid grid)
        {
            int gridRow = 0;
            int gridColumn = 0;
            Border dateBorder = AddBorderToGridCell(grid, true, true, gridRow, gridColumn);
            Grid.SetColumnSpan(dateBorder, 2);
            AddContentToGrid(dateBorder, "Date", ref gridRow, gridColumn, DateColumnStyle);

            Border remarksBorder = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
            Grid.SetColumnSpan(remarksBorder, 2);
            AddContentToGrid(grid, "Remarks", ref gridRow, gridColumn, RemarksStyle);

            Border bankBalanceBorder = AddBorderToGridCell(grid, BankBalanceBackground, false, gridRow, gridColumn);
            Grid.SetColumnSpan(bankBalanceBorder, 2);
            TextBlock bankBalanceTextBlock = AddContentToGrid(bankBalanceBorder, "Balance", ref gridRow, gridColumn, HeadingStyle);
            bankBalanceTextBlock.Foreground = FindResource(BankBalanceTextBrush) as Brush;
            bankBalanceTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            Border adjustmentsBorder = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
            Grid.SetColumnSpan(adjustmentsBorder, 2);
            TextBlock adjustmentsTextBlock = AddContentToGrid(adjustmentsBorder, "Adjustments", ref gridRow, gridColumn, HeadingStyle);
            adjustmentsTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            Border surplusBorder = AddBorderToGridCell(grid, SurplusBackground, false, gridRow, gridColumn);
            Grid.SetColumnSpan(surplusBorder, 2);
            TextBlock surplusTextBlock = AddContentToGrid(surplusBorder, "Surplus", ref gridRow, gridColumn, HeadingStyle);
            surplusTextBlock.Foreground = FindResource(SurplusTextBrush) as Brush;
            surplusTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            gridRow = 5;
            foreach (LedgerColumn ledger in this.ledgerBook.Ledgers)
            {
                gridColumn = 0;
                Border border = AddBorderToGridCell(grid, true, true, gridRow, gridColumn);
                // SpentMonthly Legders do not show the transaction total (NetAmount) because its always the same.
                Grid.SetRowSpan(border, ledger.BudgetBucket is SpentMonthlyExpenseBucket ? 1 : 2);

                // Heading stripe to indicate SpentMonthly or SavedUpFor expenses.
                var stripe = new Border
                {
                    BorderThickness = new Thickness(6, 0, 0, 0),
                    BorderBrush = StripColour(ledger),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                };
                border.Child = stripe;

                string tooltip = string.Format(CultureInfo.CurrentCulture, "{0}: {1} - {2}", ledger.BudgetBucket.TypeDescription, ledger.BudgetBucket.Code, ledger.BudgetBucket.Description);
                TextBlock ledgerTitle = AddContentToGrid(stripe, ledger.BudgetBucket.Code, ref gridRow, gridColumn, HeadingStyle, tooltip);
                ledgerTitle.HorizontalAlignment = HorizontalAlignment.Center;
                gridRow--; // Ledger heading shares a gridRow with other Ledger Headings
                gridColumn++;

                if (!(ledger.BudgetBucket is SpentMonthlyExpenseBucket))
                {
                    Border border1 = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
                    TextBlock ledgerTxnsHeading = AddContentToGrid(border1, "Txns", ref gridRow, gridColumn, HeadingStyle, "Transactions");
                    ledgerTxnsHeading.HorizontalAlignment = HorizontalAlignment.Right;
                }

                Border border2 = AddBorderToGridCell(grid, true, true, gridRow, gridColumn);
                TextBlock ledgerBalanceHeading = AddContentToGrid(border2, "Balance", ref gridRow, gridColumn, HeadingStyle, "Ledger Balance");
                ledgerBalanceHeading.HorizontalAlignment = HorizontalAlignment.Right;
            }
        }

        private TextBlock AddHyperlinkToGrid(Panel parent, string hyperlinkText, ref int gridRow, int gridColumn, string style, string tooltip = null, object parameter = null)
        {
            var hyperlink = new Hyperlink(new Run(hyperlinkText))
            {
                Command = this.showTransactionsCommand,
                CommandParameter = parameter,
            };
            var textBlock = new TextBlock(hyperlink)
            {
                Style = FindResource(style) as Style,
                ToolTip = tooltip ?? hyperlinkText,
            };
            Grid.SetColumn(textBlock, gridColumn);
            Grid.SetRow(textBlock, gridRow++);
            parent.Children.Add(textBlock);
            return textBlock;
        }

        private void AddLedgerEntryLinesVertically(Grid grid)
        {
            int gridColumn = 2; //because the first two columns are headings
            List<LedgerColumn> allLedgers = this.ledgerBook.Ledgers.ToList();
            foreach (LedgerEntryLine line in this.ledgerBook.DatedEntries)
            {
                int gridRow = 0;
                gridRow = AddDateCellToLedgerEntryLine(grid, gridRow, ref gridColumn, line);

                TextBlock remarksHyperlink = AddHyperlinkToGrid(grid, "...", ref gridRow, gridColumn, NormalStyle, line.Remarks);
                var hyperlink = (Hyperlink)remarksHyperlink.Inlines.FirstInline;
                hyperlink.Command = this.showRemarksCommand;
                hyperlink.CommandParameter = line;

                AddBorderToGridCell(grid, BankBalanceBackground, false, gridRow, gridColumn);
                TextBlock bankBalanceText = AddHyperlinkToGrid(
                    grid,
                    line.LedgerBalance.ToString("N", CultureInfo.CurrentCulture),
                    ref gridRow,
                    gridColumn,
                    ImportantNumberStyle,
                    string.Format(CultureInfo.CurrentCulture, "Ledger Balance: {0:N} Bank Balance {1:N}", line.LedgerBalance, line.TotalBankBalance),
                    line);
                hyperlink = (Hyperlink)bankBalanceText.Inlines.FirstInline;
                hyperlink.Command = this.showBankBalancesCommand;
                hyperlink.CommandParameter = line;
                bankBalanceText.Foreground = FindResource(BankBalanceTextBrush) as Brush;

                AddHyperlinkToGrid(
                    grid,
                    line.TotalBalanceAdjustments.ToString("N", CultureInfo.CurrentCulture),
                    ref gridRow,
                    gridColumn,
                    ImportantNumberStyle,
                    parameter: line);

                Border surplusBorder = AddBorderToGridCell(grid, SurplusBackground, false, gridRow, gridColumn);
                TextBlock surplusText = AddContentToGrid(surplusBorder, line.CalculatedSurplus.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, ImportantNumberStyle);
                surplusText.Foreground = FindResource(SurplusTextBrush) as Brush;


                foreach (LedgerColumn ledger in allLedgers)
                {
                    LedgerEntry entry = line.Entries.FirstOrDefault(e => e.LedgerColumn.Equals(ledger));
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

                    if (ledger.BudgetBucket is SpentMonthlyExpenseBucket)
                    {
                        AddBorderToGridCell(grid, false, true, gridRow, gridColumn);
                        AddHyperlinkToGrid(grid, balance.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, NumberStyle, parameter: entry);
                    }
                    else
                    {
                        AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
                        AddHyperlinkToGrid(grid, netAmount.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, NumberStyle, parameter: entry);
                        AddBorderToGridCell(grid, false, true, gridRow, gridColumn);
                        AddHyperlinkToGrid(grid, balance.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, NumberStyle, parameter: entry);
                    }
                }

                gridColumn++;
            }
        }

        private void AddLedgerRows(Grid grid)
        {
            foreach (LedgerColumn ledger in this.ledgerBook.Ledgers)
            {
                if (ledger.BudgetBucket is SpentMonthlyExpenseBucket)
                {
                    // Spent Monthly ledgers only have a balance gridRow
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                else
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
            }
        }

        private void DynamicallyCreateLedgerBookGrid()
        {
            if (this.ledgerBook == null)
            {
                this.contentPresenter = null;
                return;
            }

            var grid = new Grid();
            // Date
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            // Ledgers 
            AddLedgerRows(grid);
            // Surplus
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            // Adjustments
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            // Ledger Balance
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            // Remarks
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            this.contentPresenter.Content = grid;
            AddGridColumns(grid);

            AddHeadingColumnContent(grid);
            AddLedgerEntryLinesVertically(grid);
        }

        private object FindResource(string resourceName)
        {
            object localResource = this.localResources[resourceName];
            if (localResource != null)
            {
                return localResource;
            }

            return Application.Current.FindResource(resourceName);
        }
    }
}