namespace BudgetAnalyser.Engine.Account
{
    public class AmexAccount : AccountType
    {
        public AmexAccount(string name)
        {
            Name = name;
        }

        internal override string[] KeyWords
        {
            get { return new[] { "AMEX" }; }
        }

        public override string ImagePath
        {
            get { return "../Assets/AmexLogo.png"; }
        }

        public override AccountType Clone(string name)
        {
            return new AmexAccount(name);
        }
    }
}