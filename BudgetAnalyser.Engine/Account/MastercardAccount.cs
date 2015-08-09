namespace BudgetAnalyser.Engine.Account
{
    public class MastercardAccount : Account
    {
        public MastercardAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType => AccountType.CreditCard;

        public override string ImagePath => "MastercardLogoImage";

        internal virtual string[] KeyWords => new[] { "MASTERCARD" };

        public virtual Account Clone(string name)
        {
            return new MastercardAccount(name);
        }
    }
}