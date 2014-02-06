using System;

namespace BudgetAnalyser.Engine.Budget
{
    public class JournalBucket : BudgetBucket
    {
        public JournalBucket()
        {
            Id = new Guid("23839a12-ffcc-4fd1-905c-9cd2ffe3a54d");
        }

        public JournalBucket(string code, string description) : base(code, description)
        {
            Id = new Guid("23839a12-ffcc-4fd1-905c-9cd2ffe3a54d");
        }
    }
}