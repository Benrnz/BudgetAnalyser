using System;

namespace BudgetAnalyser.Engine
{
    public class BasicMapper<TSource, TDestination> 
        where TSource : class 
        where TDestination : class
    {
        public virtual TDestination Map(TSource source)
        {
            throw new NotImplementedException();
        }
    }
}