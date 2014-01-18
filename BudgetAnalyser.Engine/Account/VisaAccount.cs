namespace BudgetAnalyser.Engine.Account
{
    public class VisaAccount : AccountType
    {
        public VisaAccount(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "../Assets/VisaLogo.png"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "VISA" }; }
        }

        public override AccountType Clone(string name)
        {
            return new VisaAccount(name);
        }
    }
}