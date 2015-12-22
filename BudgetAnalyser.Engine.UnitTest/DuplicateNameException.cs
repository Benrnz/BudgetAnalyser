using System;

namespace BudgetAnalyser.UnitTest
{
    internal class DuplicateNameException : Exception
    {
        public DuplicateNameException(string message) : base(message)
        {
        }
    }
}
