namespace BudgetAnalyser.Engine.Ledger.Data
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "DataBank", Justification="Not intended to mean Databank.")]
    public class DataBankBalance
    {
        public string Account { get; set; }

        public decimal Balance { get; set; }
    }
}
