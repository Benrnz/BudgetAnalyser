namespace BudgetAnalyser.Engine.Account
{
    public class AmexAccount : Account
    {
        public AmexAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType
        {
            get { return AccountType.CreditCard; }
        }

        public override string ImagePath
        {
            get { return "AmexLogoImage"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "AMEX" }; }
        }

        public override Account Clone(string name)
        {
            return new AmexAccount(name);
        }
    }
}