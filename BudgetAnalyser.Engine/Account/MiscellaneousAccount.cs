namespace BudgetAnalyser.Engine.Account
{
    public class MiscellaneousAccount : Account
    {
        public MiscellaneousAccount(string name)
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

        public override Account Clone(string name)
        {
            return new MiscellaneousAccount(name);
        }
    }
}