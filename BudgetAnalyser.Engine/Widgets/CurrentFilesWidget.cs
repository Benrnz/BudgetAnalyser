using System;
using System.Globalization;
using System.IO;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widgets
{
    public class CurrentFilesWidget : Widget
    {
        private string budgetName;
        private string ledgerBookName;
        private string statementName;

        public CurrentFilesWidget()
        {
            Category = "2 Overview";
            Dependencies = new[] { typeof(StatementModel), typeof(IBudgetCurrencyContext), typeof(LedgerBook) };
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle1";
            Clickable = true;
            DetailedText = "No files loaded!";
            ColourStyleName = WidgetWarningStyle;
        }

        public bool HasBudget { get; private set; }
        public bool HasLedgerBook { get; private set; }
        public bool HasStatement { get; private set; }

        public override void Update([NotNull] params object[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var statementModel = input[0] as StatementModel;
            var budgetModel = input[1] as IBudgetCurrencyContext;
            var ledgerBook = input[2] as LedgerBook;

            int number = 0;
            if (statementModel == null)
            {
                this.statementName = "! No Statement file is loaded !";
            }
            else
            {
                number++;
                HasStatement = true;
                this.statementName = ShortenFileName(statementModel.FileName);
            }

            if (budgetModel == null)
            {
                this.budgetName = "! No Budget file is loaded !";
            }
            else
            {
                number++;
                HasBudget = true;
                this.budgetName = ShortenFileName(budgetModel.FileName);
            }

            if (ledgerBook == null)
            {
                this.ledgerBookName = "! No LedgerBook file is loaded !";
            }
            else
            {
                number++;
                HasLedgerBook = true;
                this.ledgerBookName = ShortenFileName(ledgerBook.FileName);
            }

            if (number == 3)
            {
                ColourStyleName = WidgetStandardStyle;
                Clickable = false;
            }
            else
            {
                Clickable = true;
                ColourStyleName = WidgetWarningStyle;
            }

            LargeNumber = number.ToString(CultureInfo.CurrentCulture);
            DetailedText = string.Format(CultureInfo.CurrentCulture, "{0}\n{1}\n{2}", this.statementName, this.budgetName, this.ledgerBookName);
        }

        private static string ShortenFileName([NotNull] string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (fileName.Length < 30)
            {
                return fileName;
            }

            string proposed = Path.GetFileName(fileName);
            string drive = Path.GetPathRoot(fileName);
            if (proposed.Length < 30)
            {
                return drive + "...\\" + proposed;
            }

            return drive + "...\\" + proposed.Substring(proposed.Length - 30);
        }
    }
}