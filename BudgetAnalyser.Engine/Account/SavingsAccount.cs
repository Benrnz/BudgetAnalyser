namespace BudgetAnalyser.Engine.Account
{
    public class SavingsAccount : AccountType
    {
        public SavingsAccount(string name)
        {
            this.Name = name;
        }

        public override string ImagePath
        {
            get { return "SavingsLogoImage"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "SAVINGS", "SAVE", "DEPOSIT", "TERM DEPOSIT", "ONCALL", "ON CALL" }; }
        }

        public override AccountType Clone(string name)
        {
            return new SavingsAccount(name);
        }
    }
}