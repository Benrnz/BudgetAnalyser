using System.Windows;
using System.Windows.Controls;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.LedgerBook
{
    public interface ILedgerBookGridBuilder
    {
        /// <summary>
        ///     This is drawn programatically because the dimensions of the ledger book grid are two-dimensional and dynamic.
        ///     Unknown number
        ///     of columns and many rows. ListView and DataGrid dont work well.
        /// </summary>
        void BuildGrid(
            [CanBeNull] Engine.Ledger.LedgerBook currentLedgerBook, 
            [NotNull] ResourceDictionary viewResources, 
            [NotNull] ContentPresenter contentPanel,
            int numberOfMonthsToShow);
    }
}