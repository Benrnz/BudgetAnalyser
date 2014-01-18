using BudgetAnalyser.Engine.Account;

namespace BudgetAnalyser.Engine
{
    public class MiscellaneousAccountType : AccountType
    {
        public MiscellaneousAccountType(string name)
        {
            Name = name;
        }

        public override string ImagePath
        {
            get { return "../Assets/Misc1Logo.png"; }
        }

        internal override string[] KeyWords
        {
            get { return new string[] { }; }
        }

        public override AccountType Clone(string name)
        {
            return new MiscellaneousAccountType(name);
        }
    }
}