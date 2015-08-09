namespace BudgetAnalyser.Engine.Account
{
    public class SavingsAccount : Account
    {
        public SavingsAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType => AccountType.Savings;

        public override string ImagePath => "SavingsLogoImage";

        internal virtual string[] KeyWords => new[] { "SAVINGS", "SAVE", "DEPOSIT", "TERM DEPOSIT", "ONCALL", "ON CALL" };

        public virtual Account Clone(string name)
        {
            return new SavingsAccount(name);
        }
    }
}