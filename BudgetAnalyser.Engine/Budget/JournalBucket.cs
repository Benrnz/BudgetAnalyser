namespace BudgetAnalyser.Engine.Budget
{
    public class JournalBucket : BudgetBucket
    {
        public const string JournalCode = "JOURNAL";

        public JournalBucket()
        {
            // Default constructor required for deserialisation.
        }

        public JournalBucket(string code, string description) : base(code, description)
        {
            // this.Id = new Guid("23839a12-ffcc-4fd1-905c-9cd2ffe3a54d");
        }
    }
}