namespace BudgetAnalyser.Engine.Budget
{
    public class PayCreditCardBucket : BudgetBucket
    {
        public const string PayCreditCardCode = "PAYCC";

        public PayCreditCardBucket()
        {
            // Default constructor required for deserialisation.
        }

        public PayCreditCardBucket(string code, string description) : base(code, description)
        {
            // this.Id = new Guid("23839a12-ffcc-4fd1-905c-9cd2ffe3a54d");
        }
    }
}