namespace BudgetAnalyser.Engine.Account
{
    public class ChequeAccount : Account
    {
        public ChequeAccount(string name)
        {
            Name = name;
        }

        public override AccountType AccountType => AccountType.General;
        public override string ImagePath => "ChequeLogoImage";
        // TODO If multiple cheque (or multiple non-savings) accounts are ever allowed, this may need to be more robust.
        public override bool IsSalaryAccount => true;
        internal virtual string[] KeyWords => new[] { "CHEQUE", "CHECK" };

        public virtual Account Clone(string name)
        {
            return new ChequeAccount(name);
        }
    }
}