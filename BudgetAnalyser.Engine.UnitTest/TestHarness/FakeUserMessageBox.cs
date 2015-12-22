using System;
using Rees.UserInteraction.Contracts;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class FakeUserMessageBox : IUserMessageBox
    {
        public void Show(string message, string headingCaption = "")
        {
            Console.WriteLine(message);
        }

        public void Show(string format, object argument1, params object[] args)
        {
            Console.WriteLine(format, argument1, args);
        }

        public void Show(string headingCaption, string format, object argument1, params object[] args)
        {
            Console.WriteLine(format, argument1, args);
        }

        public void Show(Exception ex, string message)
        {
            Console.WriteLine(message);
            Console.WriteLine(ex);
        }

        public void Show(Exception ex, string format, object argument1, params object[] args)
        {
            Console.WriteLine(format, argument1, args);
            Console.WriteLine(ex);
        }
    }
}