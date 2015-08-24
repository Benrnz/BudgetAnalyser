namespace BudgetAnalyser.Engine.BankAccount
{
    public class MiscellaneousAccount : Account
    {
        public MiscellaneousAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType => AccountType.General;
        public override string ImagePath => "../Assets/Misc1Logo.png";
        internal virtual string[] KeyWords => new string[] { };

        public virtual Account Clone(string name)
        {
            return new MiscellaneousAccount(name);
        }
    }
}