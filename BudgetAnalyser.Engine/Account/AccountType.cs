namespace BudgetAnalyser.Engine.Account
{
    public abstract class AccountType
    {
        protected AccountType()
        {
            Name = GetType().Name;
        }

        public abstract string ImagePath { get; }

        public string Name { get; protected set; }

        internal abstract string[] KeyWords { get; }

        public abstract AccountType Clone(string name);

        public override string ToString()
        {
            return Name;
        }
    }
}