using System;
using System.Collections;
using System.Reflection;
using Rees.TangyFruitMapper.Validation;

namespace Rees.TangyFruitMapper
{
    public class DictionariesAreNotSupported : PreconditionRule
    {
        public override void IsCompliant(PropertyInfo property)
        {
            if (typeof(IDictionary).IsAssignableFrom(property.PropertyType))
            {
                throw new NotSupportedException("Dictionaries are currently not supported.");
            }
        }
    }
}
