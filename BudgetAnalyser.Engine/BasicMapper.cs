using System;

namespace BudgetAnalyser.Engine
{
    public class BasicMapper<TSource, TDest> where TSource : class 
        where TDest : class
    {
        public virtual TDest Map(TSource source)
        {
            throw new NotImplementedException();
        }
    }
}