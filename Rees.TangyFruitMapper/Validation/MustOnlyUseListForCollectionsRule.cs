using System;
using System.Collections;
using System.Reflection;

namespace Rees.TangyFruitMapper.Validation
{
    internal class MustOnlyUseListForCollectionsRule : PreconditionPropertyRule
    {
        public override void IsCompliant(PropertyInfo property)
        {
            var target = property.PropertyType;
            if (target.IsCollection())
            {
                if (typeof(IDictionary).IsAssignableFrom(target))
                {
                    throw new NotSupportedException("Dictionaries are currently not supported.");
                }

                if (!target.IsConstructedGenericType)
                {
                    throw new CollectionsMustBeListTException("Dto collections must use generic lists, for example: List<string>.");
                }

                if (target.GetGenericArguments().Length != 1)
                {
                    throw new CollectionsMustBeListTException("Dto collections can only contain one generic type argument.");
                }

                if (typeof(IList).IsAssignableFrom(target))
                {
                    return;
                }

                throw new CollectionsMustBeListTException("Dto collections must only be of type List<T>.");
            }
        }
    }
}