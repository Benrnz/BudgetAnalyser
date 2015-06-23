namespace BudgetAnalyser.Engine.Account
{
    public class MastercardAccount : Account
    {
        public MastercardAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType
        {
            get { return AccountType.CreditCard; }
        }

        public override string ImagePath
        {
            get { return "MastercardLogoImage"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "MASTERCARD" }; }
        }

        public override Account Clone(string name)
        {
            return new MastercardAccount(name);
        }
    }
}