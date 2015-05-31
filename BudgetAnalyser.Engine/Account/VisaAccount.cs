namespace BudgetAnalyser.Engine.Account
{
    public class VisaAccount : Account
    {
        public VisaAccount(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "VisaLogoImage"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "VISA" }; }
        }

        public override Account Clone(string name)
        {
            return new VisaAccount(name);
        }
    }
}