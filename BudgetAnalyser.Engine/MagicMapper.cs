using System;
using AutoMapper;
using BudgetAnalyser.Engine.Annotations;

namespace BudgetAnalyser.Engine
{
    public class MagicMapper<TSource, TDestination> : BasicMapper<TSource, TDestination>
        where TSource : class
        where TDestination : class
    {
        public override TDestination Map([NotNull] TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return Mapper.Map<TDestination>(source);
        }
    }
}
