using System.Diagnostics.CodeAnalysis;

namespace BudgetAnalyser.Engine.Ledger.Data
{
    /// <summary>
    /// A Dto for <see cref="BankBalance"/>
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DataBank", Justification = "Not intended to mean Databank.")]
    public class BankBalanceDto
    {
        public string Account { get; set; }

        public decimal Balance { get; set; }
    }
}