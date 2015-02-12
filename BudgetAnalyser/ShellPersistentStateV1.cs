using System.Windows;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser
{
    public class ShellPersistentStateV1 : IPersistent
    {
        public int LoadSequence
        {
            get { return 1; }
        }

        public Point Size { get; set; }
        public Point TopLeft { get; set; }
    }
}