using System;

namespace BudgetAnalyser.UnitTest
{
    internal static class PrivateAccessor
    {
        public static void SetProperty(object target, string name, object value)
        {
            // TODO this is a stub only until the nuget is fixed for Rees.UnitTestUtilities.
            throw new NotImplementedException();
        }

        public static void SetField(object target, string name, object value)
        {
            throw new NotImplementedException();
        }
    }
}
