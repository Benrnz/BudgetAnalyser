namespace BudgetAnalyser.Engine.Account
{
    public class ChequeAccount : Account
    {
        public ChequeAccount(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "ChequeLogoImage"; }
        }

        public override bool IsSalaryAccount
        {
            get
            {
                // TODO If multiple cheque (or multiple non-savings) accounts are ever allowed, this may need to be more robust.
                return true;
            }
        }

        internal override string[] KeyWords
        {
            get { return new[] { "CHEQUE", "CHECK" }; }
        }

        public override Account Clone(string name)
        {
            return new ChequeAccount(name);
        }
    }
}