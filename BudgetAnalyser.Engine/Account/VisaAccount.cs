namespace BudgetAnalyser.Engine.Account
{
    public class VisaAccount : Account
    {
        public VisaAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType => AccountType.CreditCard;
        public override string ImagePath => "VisaLogoImage";
        internal virtual string[] KeyWords => new[] { "VISA" };

        public virtual Account Clone(string name)
        {
            return new VisaAccount(name);
        }
    }
}