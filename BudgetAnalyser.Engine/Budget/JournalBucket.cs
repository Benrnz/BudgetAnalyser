namespace BudgetAnalyser.Engine.Budget
{
    public class JournalBucket : BudgetBucket
    {
        public JournalBucket()
        {
            // Default constructor required for deserialisation.
        }

        public JournalBucket(string code, string description) : base(code, description)
        {
            // this.Id = new Guid("23839a12-ffcc-4fd1-905c-9cd2ffe3a54d");
        }

        public const string JournalCode = "JOURNAL";
    }
}