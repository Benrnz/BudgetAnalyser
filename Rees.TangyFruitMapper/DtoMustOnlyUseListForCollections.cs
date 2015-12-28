using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Rees.TangyFruitMapper
{
    internal class DtoMustOnlyUseListForCollections : DtoPreconditionRule
    {
        public override void IsCompliant(PropertyInfo property)
        {
            if (typeof(IEnumerable<>).IsAssignableFrom(property.PropertyType) || typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                if (!typeof(List<>).IsAssignableFrom(property.PropertyType))
                {
                    throw new CollectionsMustBeListTException();
                }
            }
        }
    }
}