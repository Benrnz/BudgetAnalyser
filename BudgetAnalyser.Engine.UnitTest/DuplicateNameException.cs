using System;

namespace BudgetAnalyser.Engine.UnitTest
{
    internal class DuplicateNameException : Exception
    {
        public DuplicateNameException(string message) : base(message)
        {
        }
    }
}
