using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BudgetAnalyser.Converters;
using BudgetAnalyser.Engine.BankAccount;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;

namespace BudgetAnalyser.LedgerBook;

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
    private const string MainBackground = "Brush.MainBackground";
    private const string MoreButtonStyle = "Button.Round.Add";
    private const string NormalHighlightBackground = "Brush.TileBackground";
    private const string NormalStyle = "LedgerBookTextBlockOther";
    private const string NumberStyle = "LedgerBookTextBlockNumber";
    private const string RemarksStyle = "LedgerBookTextBlockHeadingRight";
    private const string SurplusBackground = "Brush.TileBackgroundAlternate";
    private const string SurplusTextBrush = "Brush.CreditBackground1";
    private readonly ICommand showBankBalancesCommand;
    private readonly ICommand showHidePeriodsCommand;
    private readonly ICommand showLedgerBucketDetailsCommand;
    private readonly ICommand showRemarksCommand;
    private readonly ICommand showSurplusBalancesCommand;
    private readonly ICommand showTransactionsCommand;
    private ContentPresenter contentPresenter;
    private Engine.Ledger.LedgerBook ledgerBook;
    private ResourceDictionary localResources;
    private List<LedgerBucket> sortedLedgers;

    public LedgerBookGridBuilderV2(
        ICommand showTransactionsCommand,
        ICommand showBankBalancesCommand,
        ICommand showRemarksCommand,
        ICommand showHidePeriodsCommand,
        ICommand showSurplusBalancesCommand,
        ICommand showLedgerBucketDetailsCommand)
    {
        this.showTransactionsCommand = showTransactionsCommand;
        this.showBankBalancesCommand = showBankBalancesCommand;
        this.showRemarksCommand = showRemarksCommand;
        this.showHidePeriodsCommand = showHidePeriodsCommand;
        this.showSurplusBalancesCommand = showSurplusBalancesCommand;
        this.showLedgerBucketDetailsCommand = showLedgerBucketDetailsCommand;
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
        int numberOfPeriodsToShow)
    {
        if (viewResources == null)
        {
            throw new ArgumentNullException(nameof(viewResources));
        }

        if (contentPanel == null)
        {
            throw new ArgumentNullException(nameof(contentPanel));
        }

        this.ledgerBook = currentLedgerBook;
        this.localResources = viewResources;
        this.contentPresenter = contentPanel;
        DynamicallyCreateLedgerBookGrid(numberOfPeriodsToShow);
    }

    private Border AddBorderToGridCell(Panel parent, bool hasBackground, bool hasBorder, int gridRow, int gridColumn)
    {
        return AddBorderToGridCell(parent, hasBackground ? NormalHighlightBackground : MainBackground, hasBorder, gridRow, gridColumn);
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
            ToolTip = tooltip ?? content
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
            throw new ArgumentException(nameof(parent) + " is not a Panel nor a Decorator", nameof(parent));
        }

        return textBlock;
    }

    private int AddDateCellToLedgerEntryLine(Grid grid, int gridRow, ref int gridColumn, LedgerEntryLine line)
    {
        var dateBorder = AddBorderToGridCell(grid, false, true, gridRow, gridColumn);
        AddContentToGrid(dateBorder, line.Date.ToString(DateFormat, CultureInfo.CurrentCulture), ref gridRow, gridColumn, DateColumnStyle);
        return gridRow;
    }

    private void AddGridColumns(Grid grid, int numberOfPeriodsToShow)
    {
        if (numberOfPeriodsToShow < 1)
        {
            numberOfPeriodsToShow = 1;
        }

        if (numberOfPeriodsToShow > this.ledgerBook.Reconciliations.Count())
        {
            numberOfPeriodsToShow = this.ledgerBook.Reconciliations.Count();
        }

        for (var index = 0; index < numberOfPeriodsToShow + 2; index++)
        {
            // + 2 because we need 2 columns for the headings
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }
    }

    private void AddHeadingColumnContent(Grid grid)
    {
        // Date
        var gridRow = 0;
        var gridColumn = 0;
        var dateBorder = AddBorderToGridCell(grid, true, true, gridRow, gridColumn);
        Grid.SetColumnSpan(dateBorder, 2);
        AddContentToGrid(dateBorder, "Date", ref gridRow, gridColumn, DateColumnStyle);

        // Remarks
        var remarksBorder = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
        Grid.SetColumnSpan(remarksBorder, 2);
        AddContentToGrid(grid, "Remarks", ref gridRow, gridColumn, RemarksStyle);

        // Bank Balance
        var bankBalanceBorder = AddBorderToGridCell(grid, BankBalanceBackground, false, gridRow, gridColumn);
        Grid.SetColumnSpan(bankBalanceBorder, 2);
        var bankBalanceTextBlock = AddContentToGrid(bankBalanceBorder, "Balance", ref gridRow, gridColumn, HeadingStyle);
        bankBalanceTextBlock.Foreground = (Brush)FindResource(BankBalanceTextBrush);
        bankBalanceTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

        // Balance Adjustments
        var adjustmentsBorder = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
        Grid.SetColumnSpan(adjustmentsBorder, 2);
        var adjustmentsTextBlock = AddContentToGrid(adjustmentsBorder, "Adjustments", ref gridRow, gridColumn, HeadingStyle);
        adjustmentsTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

        // Surplus
        var surplusBorder = AddBorderToGridCell(grid, SurplusBackground, false, gridRow, gridColumn);
        Grid.SetColumnSpan(surplusBorder, 2);
        var surplusTextBlock = AddContentToGrid(surplusBorder, "Surplus", ref gridRow, gridColumn, HeadingStyle);
        surplusTextBlock.Foreground = (Brush)FindResource(SurplusTextBrush);
        surplusTextBlock.HorizontalAlignment = HorizontalAlignment.Right;

        // Ledgers
        gridRow = 5;
        Account currentBankAccount = null;

        foreach (var ledger in this.sortedLedgers)
        {
            if (currentBankAccount != ledger.StoredInAccount)
            {
                // Bank account group separater
                currentBankAccount = ledger.StoredInAccount;
                var bankAccountBorder = AddBorderToGridCell(grid, false, false, gridRow, 0);
                Grid.SetColumnSpan(bankAccountBorder, 2);
                Grid.SetRow(bankAccountBorder, gridRow++);
                bankAccountBorder.Child = new ContentPresenter { Content = ledger.StoredInAccount, Margin = new Thickness(5), Height = 50, Width = 100 };
            }

            gridColumn = 0;
            var border = AddBorderToGridCell(grid, true, true, gridRow, gridColumn);
            // SpentPeriodically Ledgers do not show the transaction total (NetAmount) because its always the same.
            Grid.SetRowSpan(border, ledger.BudgetBucket is SpentPerPeriodExpenseBucket ? 1 : 2);

            // Heading stripe to indicate SpentPeriodically or SavedUpFor expenses.
            var stripe = new Border
            {
                BorderThickness = new Thickness(6, 0, 0, 0),
                BorderBrush = StripColour(ledger),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };
            border.Child = stripe;

            var tooltip = string.Format(CultureInfo.CurrentCulture, "{0}: {1} - {2}", ledger.BudgetBucket.TypeDescription, ledger.BudgetBucket.Code, ledger.BudgetBucket.Description);
            var hyperlink = new Hyperlink(new Run(ledger.BudgetBucket.Code))
            {
                Command = this.showLedgerBucketDetailsCommand,
                CommandParameter = this.ledgerBook.Ledgers.Single(l => l.BudgetBucket == ledger.BudgetBucket) // Must be Main Ledger Map from book
            };
            var textBlock = new TextBlock(hyperlink)
            {
                Style = (Style)FindResource(HeadingStyle),
                ToolTip = tooltip,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stripe.Child = textBlock;

            gridColumn++;

            if (!(ledger.BudgetBucket is SpentPerPeriodExpenseBucket))
            {
                var border1 = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
                var ledgerTxnsHeading = AddContentToGrid(border1, "Txns", ref gridRow, gridColumn, HeadingStyle, "Transactions");
                ledgerTxnsHeading.HorizontalAlignment = HorizontalAlignment.Right;
            }

            var border2 = AddBorderToGridCell(grid, true, true, gridRow, gridColumn);
            var ledgerBalanceHeading = AddContentToGrid(border2, "Balance", ref gridRow, gridColumn, HeadingStyle, "Ledger Balance");
            ledgerBalanceHeading.HorizontalAlignment = HorizontalAlignment.Right;
        }
    }

    private TextBlock AddHyperlinkToGrid(Panel parent, string hyperlinkText, ref int gridRow, int gridColumn, string style, string tooltip = null, object parameter = null)
    {
        var hyperlink = new Hyperlink(new Run(hyperlinkText))
        {
            Command = this.showTransactionsCommand,
            CommandParameter = parameter
        };
        var textBlock = new TextBlock(hyperlink)
        {
            Style = (Style)FindResource(style),
            ToolTip = tooltip ?? hyperlinkText
        };
        Grid.SetColumn(textBlock, gridColumn);
        Grid.SetRow(textBlock, gridRow++);
        parent.Children.Add(textBlock);
        return textBlock;
    }

    private void AddLedgerEntryLinesVertically(Grid grid, int numberOfPeriodsToShow)
    {
        var gridColumn = 2; //because the first two columns are headings
        var periodNumber = 0;

        // Loop thru all Reconciliations from most recent to oldest adding cells to the grid vertically. 
        foreach (var line in this.ledgerBook.Reconciliations)
        {
            var gridRow = 0;
            if (++periodNumber > numberOfPeriodsToShow)
            {
                break;
            }

            // Date
            gridRow = AddDateCellToLedgerEntryLine(grid, gridRow, ref gridColumn, line);

            // Remarks
            var remarksHyperlink = AddHyperlinkToGrid(grid, "...", ref gridRow, gridColumn, NormalStyle, line.Remarks, line);
            var hyperlink = (Hyperlink)remarksHyperlink.Inlines.FirstInline;
            hyperlink.Command = this.showRemarksCommand;

            // Bank Balance
            AddBorderToGridCell(grid, BankBalanceBackground, false, gridRow, gridColumn);
            var bankBalanceText = AddHyperlinkToGrid(
                                                     grid,
                                                     line.LedgerBalance.ToString("N", CultureInfo.CurrentCulture),
                                                     ref gridRow,
                                                     gridColumn,
                                                     ImportantNumberStyle,
                                                     BuildToolTipForBankBalance(line),
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
            Account currentBankAccount = null;
            foreach (var ledger in this.sortedLedgers)
            {
                if (currentBankAccount != ledger.StoredInAccount)
                {
                    gridRow++;
                    currentBankAccount = ledger.StoredInAccount;
                }

                var entry = line.Entries.FirstOrDefault(e => e.LedgerBucket.BudgetBucket == ledger.BudgetBucket);
                decimal balance, netAmount;

                var movedBankAccounts = false;
                if (entry == null)
                {
                    // New ledger added that older entries do not have.
                    balance = 0;
                    netAmount = 0;
                }
                else if (entry.LedgerBucket != ledger)
                {
                    // This means the ledger has change bank accounts and should not be included in this bank account's group. Leave blank cells in this case.
                    movedBankAccounts = true;
                    balance = 0;
                    netAmount = 0;
                }
                else
                {
                    balance = entry.Balance;
                    netAmount = entry.NetAmount;
                }

                if (ledger.BudgetBucket is SpentPerPeriodExpenseBucket)
                {
                    var border = AddBorderToGridCell(grid, false, true, gridRow, gridColumn);
                    border.ToolTip = "This ledger has moved to another bank account.";
                    if (movedBankAccounts)
                    {
                        gridRow++;
                    }
                    else
                    {
                        AddHyperlinkToGrid(grid, balance.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, NumberStyle, parameter: entry);
                    }
                }
                else
                {
                    var border1 = AddBorderToGridCell(grid, true, false, gridRow, gridColumn);
                    var border2 = AddBorderToGridCell(grid, false, true, gridRow + 1, gridColumn);
                    if (movedBankAccounts)
                    {
                        border1.ToolTip = "This ledger has moved to another bank account.";
                        border2.ToolTip = "This ledger has moved to another bank account.";
                        gridRow += 2;
                        continue;
                    }

                    AddHyperlinkToGrid(grid, netAmount.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, NumberStyle, parameter: entry);
                    AddHyperlinkToGrid(grid, balance.ToString("N", CultureInfo.CurrentCulture), ref gridRow, gridColumn, NumberStyle, parameter: entry);
                }
            }

            gridColumn++;
        }
    }

    private void AddLedgerRows(Grid grid)
    {
        foreach (var ledger in this.sortedLedgers)
        {
            if (ledger.BudgetBucket is SpentPerPeriodExpenseBucket)
            {
                // Spent Periodically ledgers only have a balance gridRow
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            else
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
        }
    }

    private void AddShowMoreLessColumnsButtons(Grid grid, int numberOfPeriodsToShow)
    {
        grid.ColumnDefinitions.Add(new ColumnDefinition());

        var moreButton = new Button
        {
            Style = (Style)FindResource(MoreButtonStyle),
            RenderTransform = new ScaleTransform(0.5, 0.5),
            ToolTip = "Show column",
            CommandParameter = 1,
            Command = this.showHidePeriodsCommand
        };

        var lessButton = new Button
        {
            Style = (Style)FindResource(LessButtonStyle),
            RenderTransform = new ScaleTransform(0.5, 0.5),
            ToolTip = "Hide column",
            CommandParameter = -1,
            Command = this.showHidePeriodsCommand
        };

        var panel = new StackPanel
        {
            Margin = new Thickness(5, 15, 5, 5),
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        panel.Children.Add(lessButton);
        panel.Children.Add(moreButton);
        grid.Children.Add(panel);
        Grid.SetColumn(panel, numberOfPeriodsToShow + 3);
    }

    private int AddSurplusCell(Grid grid, int gridRow, int gridColumn, LedgerEntryLine line)
    {
        AddBorderToGridCell(grid, SurplusBackground, false, gridRow, gridColumn);

        var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
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
            CommandParameter = line
        };
        var textBlock = new TextBlock(hyperlink)
        {
            Style = (Style)FindResource(ImportantNumberStyle),
            ToolTip = string.Format(CultureInfo.CurrentCulture, "Total Surplus: {0:N}. Click for more detail...", line.CalculatedSurplus),
            Foreground = (Brush)FindResource(SurplusTextBrush)
        };
        stackPanel.Children.Add(textBlock);
        return ++gridRow;
    }

    private static string BuildToolTipForBankBalance(LedgerEntryLine line)
    {
        var individualLedgerBalances = new StringBuilder();
        foreach (var bankBalance in line.BankBalances)
        {
            individualLedgerBalances.AppendFormat(
                                                  CultureInfo.CurrentCulture,
                                                  "{0}: {1:N}; ",
                                                  bankBalance.Account,
                                                  bankBalance.Balance + line.BankBalanceAdjustments.Where(a => a.BankAccount == bankBalance.Account).Sum(a => a.Amount));
        }

        return string.Format(
                             CultureInfo.CurrentCulture,
                             "Total Ledger Balance: {0:N}; Adjusted Bank Balance {1:N}; {2}",
                             line.LedgerBalance,
                             line.TotalBankBalance + line.TotalBalanceAdjustments,
                             individualLedgerBalances);
    }

    private void DynamicallyCreateLedgerBookGrid(int numberOfPeriodsToShow)
    {
        if (this.ledgerBook == null)
        {
            this.contentPresenter = null;
            return;
        }

        // Sort ledgers so that the ledgers in the same bank account are grouped together
        //this.sortedLedgers = this.ledgerBook.Reconciliations.SelectMany(line => line.Entries).Select(e => e.LedgerBucket)
        // Not doing it this way anymore. I want the ledgers that are not declared in the LedgerBook to not be shown, these are old ledgers no longer used.
        this.sortedLedgers = this.ledgerBook.Ledgers
            .OrderBy(l => l.StoredInAccount.Name).ThenBy(l => l.BudgetBucket.Code)
            .Distinct()
            .ToList();

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
        AddGridColumns(grid, numberOfPeriodsToShow);

        AddHeadingColumnContent(grid);
        AddLedgerEntryLinesVertically(grid, numberOfPeriodsToShow);

        AddShowMoreLessColumnsButtons(grid, numberOfPeriodsToShow);
    }

    private object FindResource(string resourceName)
    {
        var localResource = this.localResources[resourceName];
        if (localResource != null)
        {
            return localResource;
        }

        return Application.Current.FindResource(resourceName);
    }

    private static Brush StripColour(LedgerBucket ledger)
    {
        if (ledger.BudgetBucket is SpentPerPeriodExpenseBucket)
        {
            return ConverterHelper.SpentPeriodicallyBucketBrush;
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
}