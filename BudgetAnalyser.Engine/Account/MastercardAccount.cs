namespace BudgetAnalyser.Engine.Account
{
    public class MastercardAccount : AccountType
    {
        public MastercardAccount(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "MastercardLogoImage"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "MASTERCARD" }; }
        }

        public override AccountType Clone(string name)
        {
            return new MastercardAccount(name);
        }
    }
}