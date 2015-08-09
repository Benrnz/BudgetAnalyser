namespace BudgetAnalyser.Engine.Account
{
    public class AmexAccount : Account
    {
        public AmexAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType => AccountType.CreditCard;

        public override string ImagePath => "AmexLogoImage";

        internal virtual string[] KeyWords => new[] { "AMEX" };

        public virtual Account Clone(string name)
        {
            return new AmexAccount(name);
        }
    }
}