namespace BudgetAnalyser.Engine.Account
{
    public class ChequeAccount : AccountType
    {
        public ChequeAccount(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "../Assets/ChequeLogo.png"; }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "CHEQUE", "CHECK" }; }
        }

        public override AccountType Clone(string name)
        {
            return new ChequeAccount(name);
        }
    }
}