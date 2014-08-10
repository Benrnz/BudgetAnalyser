using System;
using BudgetAnalyser.Engine;

namespace BudgetAnalyser.UnitTest.TestHarness
{
    public class BasicMapperFake<TSource, TDestination> : BasicMapper<TSource, TDestination> 
        where TDestination : class 
        where TSource : class
    {
        public override TDestination Map(TSource source)
        {
            throw new NotSupportedException();
        }
    }
}
