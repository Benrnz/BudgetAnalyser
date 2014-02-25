using System;
using System.Globalization;
using System.IO;
using BudgetAnalyser.Engine.Annotations;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Widget
{
    public class CurrentFilesWidget : Widget
    {
        private string budgetName;
        private string ledgerBookName;
        private string statementName;

        public CurrentFilesWidget()
        {
            Category = "Current Files";
            Dependencies = new[] { typeof(StatementModel), typeof(BudgetCurrencyContext), typeof(LedgerBook) };
            Size = WidgetSize.Medium;
            WidgetStyle = "ModernTileMediumStyle1";
            Clickable = true;
        }

        public bool HasBudget { get; private set; }
        public bool HasLedgerBook { get; private set; }
        public bool HasStatement { get; private set; }

        public override void Update(params object[] input)
        {
            if (!ValidateUpdateInput(input))
            {
                return;
            }

            var statementModel = input[0] as StatementModel;
            var budgetModel = input[1] as BudgetCurrencyContext;
            var ledgerBook = input[2] as LedgerBook;

            if (statementModel == null)
            {
                this.statementName = null;
            }
            else
            {
                this.statementName = ShortenFileName(statementModel.FileName);
            }

            if (budgetModel == null)
            {
                this.budgetName = null;
            }
            else
            {
                this.budgetName = ShortenFileName(budgetModel.FileName);
            }

            if (ledgerBook == null)
            {
                this.ledgerBookName = null;
            }
            else
            {
                this.ledgerBookName = ShortenFileName(ledgerBook.FileName);
            }

            int number = 0;
            if (!string.IsNullOrWhiteSpace(this.statementName))
            {
                number++;
                HasStatement = true;
            }

            if (!string.IsNullOrWhiteSpace(this.budgetName))
            {
                number++;
                HasBudget = true;
            }

            if (!string.IsNullOrWhiteSpace(this.ledgerBookName))
            {
                number++;
                HasLedgerBook = true;
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
            DetailedText = string.Format("{0}\n{1}\n{2}", this.statementName, this.budgetName, this.ledgerBookName);
        }

        private string ShortenFileName([NotNull] string fileName)
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