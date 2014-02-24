using System;

namespace BudgetAnalyser.Engine.Statement
{
    public class NamedTransaction : TransactionType
    {
        private readonly string name;

        public NamedTransaction(string name, decimal sign = 1)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            this.name = name;
            Sign = sign;
        }

        public override string Name
        {
            get { return this.name; }
        }

        public decimal Sign { get; private set; }

        public override int GetHashCode()
        {
            return this.name.GetHashCode();
        }
    }
}