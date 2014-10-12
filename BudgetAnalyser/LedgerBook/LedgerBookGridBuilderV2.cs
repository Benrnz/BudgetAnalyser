using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xaml;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine.Account;
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
        private const string LessButtonStyle = "Button.Round.Minus";
        private const string LightBorderBrush = "Brush.BorderLight";
        private const string MoreButtonStyle = "Button.Round.Add";

        private const string NormalHighlightBackground = "Brush.TileBackground";
        private const string NormalStyle = "LedgerBookTextBlockOther";
        private const string NumberStyle = "LedgerBookTextBlockNumber";
        private const string RemarksStyle = "LedgerBookTextBlockHeadingRight";
        private const string SurplusBackground = "Brush.TileBackgroundAlternate";
        private const string SurplusTextBrush = "Brush.CreditBackground1";
        private readonly ICommand removeLedgerEntryLineCommand;
        private readonly ICommand showBankBalancesCommand;
        private readonly ICommand showHideMonthsCommand;
        private readonly ICommand showRemarksCommand;
        private readonly ICommand showSurplusBalancesCommand;
        private readonly ICommand showTransactionsCommand;
        private ContentPresenter contentPresenter;
        private Engine.Ledger.LedgerBook ledgerBook;
        private ResourceDictionary localResources;
        private List<LedgerColumn> sortedLedgers;

        public LedgerBookGridBuilderV2(
            ICommand showTransactionsCommand,
            ICommand showBankBalancesCommand,
            ICommand showRemarksCommand,
            ICommand removeLedgerEntryLineCommand,
            ICommand showHideMonthsCommand,
            ICommand showSurplusBalancesCommand)
        {
            this.showTransactionsCommand = showTransactionsCommand;
            this.showBankBalancesCommand = showBankBalancesCommand;
            this.showRemarksCommand = showRemarksCommand;
            this.removeLedgerEntryLineCommand = removeLedgerEntryLineCommand;
            this.showHideMonthsCommand = showHideMonthsCommand;
            this.showSurplusBalancesCommand = showSurplusBalancesCommand;
        }

        /// <summary>
        ///     This is drawn programatically because the dimensions of the ledger book grid are two-dimensional and dynamic.
        ///     Unknown number
        ///     of columns and many rows. ListView and DataGrid dont work well.
        /// </summary>
        public void BuildGrid(
            Engine.Ledger.LedgerBook currentLedgerBook,
            ResourceDictionary viewResources,
            ContentPresenter contentPanel,
            int numberOfMonthsToShow)
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
            DynamicallyCreateLedgerBookGrid(numberOfMonthsToShow);
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
                border.Background = (Brush)FindResource(background);
            }

            if (hasBorder)
            {
                border.BorderBrush = (Brush)FindResource(LightBorderBrush);
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
                Style = (Style)FindResource(style),
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

        private void AddGridColumns(Grid grid, int numberOfMonthsToShow)
        {
            if (numberOfMonthsToShow < 1)
            {
                numberOfMonthsToShow = 1;
            }
            if (numberOfMonthsToShow > this.ledgerBook.DatedEntries.Count())
            {
                numberOfMonthsToShow = this.ledgerBook.DatedEntries.Count();
            }

            for (int index = 0; index < numberOfMonthsToShow + 2; index++)
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
            bankBalanceTextBlock.Foreground = (Brush)FindResource(BankBalanceTextBrush);
            bankBalanceTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            Border adjustmentsBorder = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
            Grid.SetColumnSpan(adjustmentsBorder, 2);
            TextBlock adjustmentsTextBlock = AddContentToGrid(adjustmentsBorder, "Adjustments", ref gridRow, gridColumn, HeadingStyle);
            adjustmentsTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            Border surplusBorder = AddBorderToGridCell(grid, SurplusBackground, false, gridRow, gridColumn);
            Grid.SetColumnSpan(surplusBorder, 2);
            TextBlock surplusTextBlock = AddContentToGrid(surplusBorder, "Surplus", ref gridRow, gridColumn, HeadingStyle);
            surplusTextBlock.Foreground = (Brush)FindResource(SurplusTextBrush);
            surplusTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

            gridRow = 5;
            AccountType currentBankAccount = null;
            foreach (LedgerColumn ledger in this.sortedLedgers)
            {
                if (currentBankAccount != ledger.StoredInAccount)
                {
                    currentBankAccount = ledger.StoredInAccount;
                    Border bankAccountBorder = AddBorderToGridCell(grid, false, false, gridRow, 0);
                    Grid.SetColumnSpan(bankAccountBorder, 2);
                    Grid.SetRow(bankAccountBorder, gridRow++);
                    bankAccountBorder.Child = new ContentControl { Template = (ControlTemplate)FindResource(ledger.StoredInAccount.ImagePath), Margin = new Thickness(5), Height = 50, Width = 100 };
                }

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
                Style = (Style)FindResource(style),
                ToolTip = tooltip ?? hyperlinkText,
            };
            Grid.SetColumn(textBlock, gridColumn);
            Grid.SetRow(textBlock, gridRow++);
            parent.Children.Add(textBlock);
            return textBlock;
        }

        private void AddLedgerEntryLinesVertically(Grid grid, int numberOfMonthsToShow)
        {
            int gridColumn = 2; //because the first two columns are headings
            int monthNumber = 0;

            // Loop thru all DatedEntries from most recent to oldest adding cells to the grid vertically. 
            foreach (LedgerEntryLine line in this.ledgerBook.DatedEntries)
            {
                int gridRow = 0;
                if (++monthNumber > numberOfMonthsToShow)
                {
                    break;
                }

                // Date
                gridRow = AddDateCellToLedgerEntryLine(grid, gridRow, ref gridColumn, line);

                // Remarks
                TextBlock remarksHyperlink = AddHyperlinkToGrid(grid, "...", ref gridRow, gridColumn, NormalStyle, line.Remarks, line);
                var hyperlink = (Hyperlink)remarksHyperlink.Inlines.FirstInline;
                hyperlink.Command = this.showRemarksCommand;

                // Bank Balance
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
                bankBalanceText.Foreground = (Brush)FindResource(BankBalanceTextBrush);

                // Balance Adjustments
                AddHyperlinkToGrid(
                    grid,
                    line.TotalBalanceAdjustments.ToString("N", CultureInfo.CurrentCulture),
                    ref gridRow,
                    gridColumn,
                    ImportantNumberStyle,
                    parameter: line);

                // Surplus
                gridRow = AddSurplusCell(grid, gridRow, gridColumn, line);

                // Ledgers
                AccountType currentBankAccount = null;
                foreach (LedgerColumn ledger in this.sortedLedgers)
                {
                    if (currentBankAccount != ledger.StoredInAccount)
                    {
                        gridRow++;
                        currentBankAccount = ledger.StoredInAccount;
                    }

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

        private int AddSurplusCell(Grid grid, int gridRow, int gridColumn, LedgerEntryLine line)
        {
            AddBorderToGridCell(grid, SurplusBackground, false, gridRow, gridColumn);
 
            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            grid.Children.Add(stackPanel);
            Grid.SetRow(stackPanel, gridRow);
            Grid.SetColumn(stackPanel, gridColumn);

            if (line.SurplusBalances.Any(b => b.Balance < 0))
            {
                var warningImage = new ContentControl
                {
                    Template = (ControlTemplate)FindResource("WarningImage"),
                    Width = 20,
                    Height = 20,
                    Margin = new Thickness(5),
                    ToolTip = "There are one or more negative surplus balances!"
                };
                stackPanel.Children.Add(warningImage);
            }

            var hyperlink = new Hyperlink(new Run(line.CalculatedSurplus.ToString("N", CultureInfo.CurrentCulture)))
            {
                Command = this.showSurplusBalancesCommand,
                CommandParameter = line,
            };
            var textBlock = new TextBlock(hyperlink)
            {
                Style = (Style)FindResource(ImportantNumberStyle),
                ToolTip = string.Format(CultureInfo.CurrentCulture, "Total Surplus: {0:N}. Click for more detail...", line.CalculatedSurplus),
                Foreground = (Brush)FindResource(SurplusTextBrush),
            };
            stackPanel.Children.Add(textBlock);
            return ++gridRow;
        }

        private void AddLedgerRows(Grid grid)
        {
            foreach (LedgerColumn ledger in this.sortedLedgers)
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

        private void AddShowMoreLessColumnsButtons(Grid grid, int numberOfMonthsToShow)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var moreButton = new Button
            {
                Style = (Style)FindResource(MoreButtonStyle),
                RenderTransform = new ScaleTransform(0.5, 0.5),
                ToolTip = "Show column",
                CommandParameter = 1,
                Command = this.showHideMonthsCommand,
            };

            var lessButton = new Button
            {
                Style = (Style)FindResource(LessButtonStyle),
                RenderTransform = new ScaleTransform(0.5, 0.5),
                ToolTip = "Hide column",
                CommandParameter = -1,
                Command = this.showHideMonthsCommand,
            };

            var panel = new StackPanel
            {
                Margin = new Thickness(5, 15, 5, 5),
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(lessButton);
            panel.Children.Add(moreButton);
            grid.Children.Add(panel);
            Grid.SetColumn(panel, numberOfMonthsToShow + 3);
        }

        private void DynamicallyCreateLedgerBookGrid(int numberOfMonthsToShow)
        {
            if (this.ledgerBook == null)
            {
                this.contentPresenter = null;
                return;
            }

            // Sort ledgers so that the ledgers in the same bank account are grouped together
            this.sortedLedgers = this.ledgerBook.Ledgers.OrderBy(l => l.StoredInAccount.Name).ThenBy(l => l.BudgetBucket.Code).ToList();

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
            // Bank account heading lines
            this.sortedLedgers.Select(l => l.StoredInAccount).Distinct().ToList().ForEach(x => grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }));

            this.contentPresenter.Content = grid;
            AddGridColumns(grid, numberOfMonthsToShow);

            AddHeadingColumnContent(grid);
            AddLedgerEntryLinesVertically(grid, numberOfMonthsToShow);

            AddShowMoreLessColumnsButtons(grid, numberOfMonthsToShow);
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