namespace BudgetAnalyser.Engine.Account
{
    public class SavingsAccount : Account
    {
        public SavingsAccount(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "SavingsLogoImage"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "SAVINGS", "SAVE", "DEPOSIT", "TERM DEPOSIT", "ONCALL", "ON CALL" }; }
        }

        public override Account Clone(string name)
        {
            return new SavingsAccount(name);
        }
    }
}