using System.Threading.Tasks;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    ///     An interface to describe operations required to produce and upload the mobile data file.
    /// </summary>
    public interface IMobileDataExporter
    {
        /// <summary>
        ///     Create the export object
        /// </summary>
        /// <returns>An object containing the summarised data.</returns>
        SummarisedLedgerMobileData CreateExportObject(StatementModel transactions, BudgetModel currentBudget, LedgerBook ledger, GlobalFilterCriteria filter);

        /// <summary>
        ///     Save a copy of the data export file locally.
        /// </summary>
        Task SaveCopyAsync(SummarisedLedgerMobileData dataObject);

        /// <summary>
        ///     Serialise the data
        /// </summary>
        string Serialise(SummarisedLedgerMobileData dataExport);
    }
}