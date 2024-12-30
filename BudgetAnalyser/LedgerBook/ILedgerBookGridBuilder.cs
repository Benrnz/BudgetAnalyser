using System.Windows;
using System.Windows.Controls;

namespace BudgetAnalyser.LedgerBook
{
    public interface ILedgerBookGridBuilder
    {
        /// <summary>
        ///     This is drawn programatically because the dimensions of the ledger grid are two-dimensional and dynamic. Unknown number of columns and many rows. ListView and DataGrid dont work well.
        /// </summary>
        void BuildGrid(
            Engine.Ledger.LedgerBook? currentLedgerBook,
            [NotNull] ResourceDictionary viewResources,
            [NotNull] ContentPresenter contentPanel,
            int numberOfPeriodsToShow);
    }
}
