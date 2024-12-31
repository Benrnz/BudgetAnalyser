using System.Windows;
using BudgetAnalyser.Engine.Persistence;

namespace BudgetAnalyser
{
    public class ShellPersistentState : IPersistentApplicationStateObject
    {
        public int LoadSequence => 1;
        public Point Size { get; set; }
        public Point TopLeft { get; set; }
    }
}
