using System;
using System.Collections.Generic;
using System.Linq;
using BudgetAnalyser.Engine.Budget;
using BudgetAnalyser.Engine.Ledger;
using BudgetAnalyser.Engine.Statement;

namespace BudgetAnalyser.Engine.Mobile
{
    /// <summary>
    /// A class to extract and summarise data to upload to the web to serve to the mobile app.
    /// </summary>
    [AutoRegisterWithIoC(SingleInstance = true)]
    public class MobileDataExporter
    {
        /// <summary>
        /// Create the export object
        /// </summary>
        /// <param name="transactions">The current <see cref="StatementModel"/></param>
        /// <param name="currentBudget">The current <see cref="BudgetModel"/></param>
        /// <param name="ledger">The current <see cref="LedgerBook"/></param>
        /// <returns>An object containing the summarised data.</returns>
        public SummarisedLedgerMobileData CreateExportObject(StatementModel transactions, BudgetModel currentBudget, LedgerBook ledger)
        {
            var export = new SummarisedLedgerMobileData
            {
                Exported = DateTime.Now,
                LastTransactionImport = transactions.LastImport,
                Title = currentBudget.Name
            };

            var latestRecon = ledger.Reconciliations.LastOrDefault();
            if (latestRecon == null) return null;

            var ledgerList = new List<SummarisedLedgerBucket>();
            foreach (var entry in latestRecon.Entries)
            {
                ledgerList.Add(new SummarisedLedgerBucket
                {
                    Balance = entry.Balance,
                    BucketCode = entry.LedgerBucket.BudgetBucket.Code,
                    BucketType = entry.LedgerBucket.BudgetBucket.TypeDescription,
                    Description = entry.LedgerBucket.BudgetBucket.Description,
                    MonthlyBudgetAmount = currentBudget.Expenses.First(e => e.Bucket.Code == entry.LedgerBucket.BudgetBucket.Code).Amount
                });
            }

            export.LedgerBuckets = ledgerList.OrderBy(l => l.BucketCode).ToList();
            return export;
        }
    }
}